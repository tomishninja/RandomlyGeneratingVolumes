// Stippling Parameters
float _StippleThreashold;
float4 _EffectColor;
float _StippleSize;

float3 _StippleHashLineA;
float3 _StippleHashLineB;
float3 _StippleHashLineC;

// https://www.shadertoy.com/view/4dffRH
// Note change this somehow tom
float3 stippleHash(float3 p)
{
	p = float3(dot(p, _StippleHashLineA),
		dot(p, _StippleHashLineB),
		dot(p, _StippleHashLineC));

	return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

float3 random(float3 uv)
{
	uv = frac(uv * float3(.1031, .1030, .0973));
	uv += dot(uv, uv.yxz + 33.33);
	return frac((uv.xxy + uv.yxx)*uv.zyx);
}

float BlueNoise(float3 U, out float distanceFromCenter, int layer, float distanceThoughTheVolume) 
{
	float3 NotRounded = U * _Resolution * layer;
	U = float3(round(NotRounded.x), round(NotRounded.y), round(NotRounded.z));
	float v = 0.;
	for (uint k = 0; k < 27; k++)
		v += stippleHash(U + float3((k % 3) - 1, ((k / 3) % 3) - 1, k / 9 - 1));

	// Not part of the blue noise algorithm but knowing the distance from the center is important.
	float3 center = random(U);
	center -= floor(center);
	float amountOfAllowedMovement = max(0.2 - _StippleSize, 0);
	float3 allowedToMoveWithin = float3(amountOfAllowedMovement, amountOfAllowedMovement, amountOfAllowedMovement);
	center = lerp(-allowedToMoveWithin, allowedToMoveWithin, center);
	//float3 center = float3(0, 0, 0);

	distanceFromCenter = distance(float3(0,0,0), center + float3(abs(U.x - NotRounded.x), abs(U.y - NotRounded.y), abs(U.z - NotRounded.z)));
	//distanceFromCenter = distance(float3(0, 0, 0), float3(abs(U.x - NotRounded.x), abs(U.y - NotRounded.y), abs(U.z - NotRounded.z)));

	// End of not part of blue noise tangenent

	return       1.125*stippleHash(U)- v / 8.  + .5; // some overbound, closer contrast
  //return .9 *(1.125*stippleHash(U) - v / 8.) + .5; // 
  //return .75*( 1.125*stippleHash(U)- v / 8.) + .5; // very slight overbound
	//return .65 * (1.125 * stippleHash(U) - v / 8.); // dimmed, but histo just fit without clamp. flat up to .5 +- .23
}

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
	// the absolute lowest distance currently avalilbe
	float minDistance = _MaxDistance;
	// holds the values from the blue noise function that tell the system where this point is in relation to the center of the voxel created
	float distanceFromCenter = 1;

	// loop until the ray is complete
	[loop]
	for (steps = 0; steps < _MaxSteps && t < _MaxDistance; steps++)
	{
		// Run the SDF calculation
		float d = SDF(pos, currentIndex);

		// if we hit some thing take note of it
		bool withinRange = d < 0;

		float dotp = dot(normal(currentIndex, pos), normalizedRayDirection);

		// Caluate the stipple
		//float stipple = ValueNoise3d(pos * (_Resolution * 2));
		// I swaped out the noise for this because the other wasn't random enough
		//float sizeMultipler = ((1 - lerp(tnear, tfar, t)) + ((_Importance[currentIndex] / 3))) / 2;
		// + (1 + (_SphereDetails[currentIndex].w - _SphereDetails[0].w));//(_Importance[currentIndex] / 2);

		float stipple = BlueNoise(pos, distanceFromCenter, 2 - (_Importance[currentIndex] / 2), 1 - (lerp(tnear, tfar, t) / 3));

		// calculate the hashing with the product of the the "inverse dot product" and "the Sphere's radius"
		bool hasEffect = withinRange && 
			d > (-_Tolerance * 1.5) && 
			dotp < 0 &&
			stipple > _StippleThreashold * (1 - (_Importance[currentIndex] / 20)) &&
			abs(dotp) < ((1 + _Importance[currentIndex]) / 2.5) &&
			lerp(0, 0.5, distanceFromCenter) < _StippleSize * (( 3 + _Importance[currentIndex]) / 3);

		voxelColor = hasEffect ? _EffectColor : withinRange ? _Colors[currentIndex] : float4(0, 0, 0, 0);

		voxelColor.a *= hasEffect ? (1 + abs(dotp)) * (1 - lerp(tnear, tfar, t)) : 1;

		voxelColor.rgb = DistanceBasedIntencity(tnear, tfar, t, voxelColor.rgb);

		// If we hit something of note then fix up the first tint or else Blend the already existing colors
		outputColor = blendingLogic(outputColor, voxelColor, hasEffect, withinRange);

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

	//return color;
	return outputColor;
}