

#ifndef AMOUNT_OF_LAYERS
#define AMOUNT_OF_LAYERS 3
#endif

// Hatching Details
float4 _GridSpaceCenter;
float3 _GridSpaceDimentions;
float3 _GridDimentions;
float4 _EffectColor;
float4x4 _GridDirectionMatrix; // allows me to set the position of the hashing exactly
float4x4 _GridDirectionMatrixArray[AMOUNT_OF_LAYERS];
float _HatchingTaperDivsor;


float SDFHashing(float3 p, float multiplier) 
{
	float3 correctGD = _GridDimentions.xyz * multiplier;

	// Transform the current postito the grid space _GridDirectionMatrix
	p = ((abs(mul(_GridDirectionMatrix, p)) % correctGD) - (correctGD / 2.0));

	// work out where this box sits in this grid
	return sdBox(p, _GridSpaceCenter.xyz * multiplier, correctGD);
}

float SDFHashing(float3 p, float multiplier, int importance, float dotp) 
{
	float3 correctGD = _GridDimentions.xyz * (multiplier);

	// Transform the current postito the grid space _GridDirectionMatrix
	p = ((abs(mul(_GridDirectionMatrixArray[importance], p)) % correctGD) - (correctGD / 2.0));

	//(abs(dotp) / (1 + _Importance[currentIndex]) - _HatchingTaperDivsor)

	// work out where this box sits in this grid
	return sdBox(p, _GridSpaceCenter.xyz * multiplier, _GridSpaceDimentions.xyz * multiplier);
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
	// the absolute lowest distance currently avalilbe
	float minDistance = _MaxDistance;


	// loop until the ray is complete
	[loop]
	for (steps = 0; steps < _MaxSteps && t < _MaxDistance; ++steps)
	{
		// Run the SDF calculation
		float d = SDF(pos, currentIndex);

		// if we hit some thing take note of it
		bool withinRange = d < 0;

		float dotp = dot(normal(currentIndex, pos), normalizedRayDirection);

		// calculate the hashing with the product of the the "inverse dot product" and "the Sphere's radius"
		// / (_HatchingTaperDivsor * (1 + (_Importance[currentIndex])))
		float hatching = SDFHashing(pos, (1 + (_SphereDetails[currentIndex].w) * 4), _Importance[currentIndex], -dotp);

		bool hasEffect = d < (_Tolerance / 2) && d > -(_Tolerance / 2) && hatching > 0 && dotp < 0 && abs(dotp) / (1 + (_Importance[currentIndex] / 4)) < _HatchingTaperDivsor;

		// Select the color so it can be blended
		voxelColor = hasEffect ?  _EffectColor : withinRange ? _Colors[currentIndex] : float4(0, 0, 0, 0);

		hasEffect = hasEffect && dotp < 0;

		//voxelColor.a *= abs(dotp) / (1 + (_Importance[currentIndex] / 4)) < _HatchingTaperDivsor < _HatchingTaperDivsor, 0 : 1;

		//float4 blendColor = blend(outputColor * (1 - _NewColorIntensity), voxelColor * (1 + (withinRange ? (_MinDistanceForStep / d) * (_NewColorIntensity) : 0)));
		// If we hit something of note then fix up the first tint or else Blend the already existing colors

		voxelColor.rgb = DistanceBasedIntencity(tnear, tfar, t, voxelColor.rgb);

		// For some reason this line is causing a compiler error to unity
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