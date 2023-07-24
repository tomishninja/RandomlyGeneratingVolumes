//fixed4 frag(v2f i, out float outDepth : SV_Depth) : SV_Target
fixed4 frag(v2f i) : SV_Target
{
	/// Starting Pos
	//float3 sPos = mul(unity_WorldToObject, float4(i.rayOrigin.xyz, 1)).xyz;
	/// The direction the ray is traveling
	float3 normalizedRayDirection = normalize(i.rayDirection);
	/// Sets the object in a AABB for me to work with for the duration of the ray
	float3 aabbMin = float3(-0.5, -0.5, -0.5);
	float3 aabbMax = float3(0.5, 0.5, 0.5);

	// get view space position of vertex
	float3 viewPos = UnityObjectToViewPos(i.vertex);

	// Cacluate the logical start and end of this ray
	float tnear;
	float tfar;
	intersect(i.local.xyz, normalizedRayDirection, aabbMax, aabbMin, tnear, tfar);

	// Cacluate the start and end identifiers
	//float3 start = sPos + normalizedRayDirection * tnear;
	//float3 end = sPos + normalizedRayDirection * tfar;
	/// The current distance traveled
	float t = tfar;

	float dist = tfar - tnear;

	//
	// Veribles for the loop below
	//
	// Pos Marker
	float3 pos = i.local + normalizedRayDirection * t;
	/// The index of the nearest volume
	int currentIndex = -1;
	/// This index must be beat to change the prioity of the object being rendered
	int bestIndexFound = currentIndex;
	/// should the color be clear this time or is the index valid?
	bool withinRange = false;
	/// The amount of steps the algorhtim needs to go until it finishes
	int steps = 0;
	/// The current color of a voxel
	float4 voxelColor = float4(0, 0, 0, 0);
	/// The final output color
	float4 outputColor = float4(0, 0, 0, 0);
	/// The closest a ray came to a object
	float clostestDistance = _MaxDistance;
	// The current index to look at for the color
	int colorIndex = 0;

	// loop until the ray is complete
	[loop]
	for (steps = 0; steps < _MaxSteps && t < _MaxDistance; ++steps)
	{
		// Run the SDF calculation
		float d = SDF(pos, currentIndex);

		// if we hit some thing take note of it
		bool withinRange = d < 0;

		// Select the color so it can be blended
		voxelColor = withinRange ? _Colors[currentIndex] : float4(0, 0, 0, 0);

		voxelColor.rgb = DistanceBasedIntencity(tnear, tfar, t, voxelColor.rgb);
		// If we hit something of note then fix up the first tint or else Blend the already existing colors
		//float percentThough = 1 + ((lerp(tnear, tfar, t) - 0.5) * 0.15);
		//voxelColor.rgb = DistanceBasedIntencity(tnear, tfar, t, voxelColor.rgb);
		outputColor = blendingLogic(outputColor, voxelColor, false, withinRange);

		// moving too slow could be bad since this object is transperent so we fix it by doing this
		d = max(abs(d), _MinDistanceForStep);

		// Move one step
		t = cacluateNextStep(t, voxelColor, d);

		// Increment the movement Logic
		pos = i.local + (t * normalizedRayDirection);

		// we only want to save the best index
		bestIndexFound = determineBestIndexFound(withinRange, false, currentIndex, bestIndexFound);

		if (t <= tnear) break;
	}

	return outputColor;
}