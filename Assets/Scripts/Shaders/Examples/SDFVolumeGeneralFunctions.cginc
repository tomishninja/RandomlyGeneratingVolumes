//
// Shader Functions and varibles that are shared over
// The various SDF volume rendering fucntctions
//

#ifndef AMOUNT_OF_COLORS
#define AMOUNT_OF_COLORS 30
#endif

#ifndef TYPE_OF_NORMAL
#define TYPE_OF_NORMAL 1
#endif

// https://iquilezles.org/articles/distfunctions/
float Sphere(float3 p, float3 c, float r)
{
	return length(p - c) - r;
}

float ClosestPointOnSphere(float3 p, float3 c, float r) {
	float3 dir = normalize(p - c);
	return c + (dir * r);
}

// https://iquilezles.org/articles/distfunctions/
float sdBox(float3 p, float3 c, float3 b)
{
	float3 q = abs(p - c) - b;
	return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	UNITY_POSITION(pos);
	float3 rayOrigin : TEXCOORD0;
	float3 rayDirection : TEXCOORD1;
	float3 local : TEXCOORD2;
	float4 vertex : TEXCOORD3;
	float4 worldSpacePos : TEXCOORD4;
	UNITY_VERTEX_OUTPUT_STEREO
};

// xyz = pos and w = radius
float4 _SphereDetails[AMOUNT_OF_COLORS];
float4 _Colors[AMOUNT_OF_COLORS];
float _Importance[AMOUNT_OF_COLORS];

// General SDF details
float _Tolerance;
float _MaxDistance;
int _MaxSteps;
float _MinDistanceForStep;
float _NewColorIntensity;
float _NormalApproxStep;
float _ColorThreshold;
int _Resolution;

// General SDF details
float _ResulitionForGradient;
float _NoiseSuppression;


// Details for the hash
float3 _HashLineA;
float3 _HashLineB;
float3 _HashLineC;

// https://www.shadertoy.com/view/4dffRH
float3 hash(float3 p)
{
	p = float3(dot(p, _HashLineA),
		dot(p, _HashLineB),
		dot(p, _HashLineC));

	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

//https://iquilezles.org/articles/gradientnoise/
// returns 3D value noise
float noise(float3 x)
{
	x *= _ResulitionForGradient;
	// grid
	float3 p = floor(x);
	float3 w = frac(x);

	// quintic interpolant
	float3 u = w * w*w*(w*(w*6.0 - 15.0) + 10.0);


	// gradients
	float3 ga = hash(p + float3(0.0, 0.0, 0.0));
	float3 gb = hash(p + float3(1.0, 0.0, 0.0));
	float3 gc = hash(p + float3(0.0, 1.0, 0.0));
	float3 gd = hash(p + float3(1.0, 1.0, 0.0));
	float3 ge = hash(p + float3(0.0, 0.0, 1.0));
	float3 gf = hash(p + float3(1.0, 0.0, 1.0));
	float3 gg = hash(p + float3(0.0, 1.0, 1.0));
	float3 gh = hash(p + float3(1.0, 1.0, 1.0));

	// projections
	float va = dot(ga, w - float3(0.0, 0.0, 0.0));
	float vb = dot(gb, w - float3(1.0, 0.0, 0.0));
	float vc = dot(gc, w - float3(0.0, 1.0, 0.0));
	float vd = dot(gd, w - float3(1.0, 1.0, 0.0));
	float ve = dot(ge, w - float3(0.0, 0.0, 1.0));
	float vf = dot(gf, w - float3(1.0, 0.0, 1.0));
	float vg = dot(gg, w - float3(0.0, 1.0, 1.0));
	float vh = dot(gh, w - float3(1.0, 1.0, 1.0));

	// interpolation
	return (va +
		u.x*(vb - va) +
		u.y*(vc - va) +
		u.z*(ve - va) +
		u.x*u.y*(va - vb - vc + vd) +
		u.y*u.z*(va - vc - ve + vg) +
		u.z*u.x*(va - vb - ve + vf) +
		u.x*u.y*u.z*(-va + vb + vc - vd + ve - vf - vg + vh)) / (_ResulitionForGradient * _NoiseSuppression);
}


// does not cacluate the index value
float SDF(int index, float3 p)
{
	// this only exists to ensure the index can't be less than zero
	index = index < 0 ? 0 : index;

	// return the final sdf
	return Sphere(p, _SphereDetails[index].xyz, _SphereDetails[index].w) - (noise(p));
}

// Cacluates the index value as well
float SDF(float3 p, out int index)
{
	float3 offset = float3(0.5, 0.5, 0.5);
	float n = noise(p);
	//float bestDistanceClosest = ClosestPointOnSphere(p + offset, _SphereDetails[0].xyz + offset, _SphereDetails[0].w + offset) - n;
	float bestDistance = Sphere(p, _SphereDetails[0].xyz, _SphereDetails[0].w) - n;
	index = 0;
	float importance = _Importance[0];

	[unroll]
	for (int i = 1; i < int(AMOUNT_OF_COLORS); i++)
	{
		//float dc = ClosestPointOnSphere(p + offset, _SphereDetails[i].xyz + offset, _SphereDetails[i].w + offset) - n;
		float d = Sphere(p, _SphereDetails[i].xyz, _SphereDetails[i].w) - n;

		//bool isThisTheBestDistance = ((abs(d) > _Tolerance || bestDistance > _Tolerance) && (d > bestDistance && d < _Tolerance)) || ((abs(d) < _Tolerance && bestDistance < _Tolerance) && _SphereDetails[i].w < _SphereDetails[index].w);// *max(1, (_Importance[i] - (_SphereDetails[i].w * _Importance[i])));
		bool isThisTheBestDistance = (d > bestDistance) && d < 0;

		index = isThisTheBestDistance ? i : index;
		bestDistance = isThisTheBestDistance ? d : bestDistance;
		//bestDistanceClosest = isThisTheBestDistance ? dc : bestDistanceClosest;
	}
	// return the closest distance
	return bestDistance;
}

// artifcually perform depth mapping
float Depth(float3 pos)
{
	float4 clipPos = UnityObjectToClipPos(pos);
	return clipPos.z / clipPos.w;
}

// 3D noise function:
// From
//Jarzynski, M., & Olano, M. (2020). Hash Functions for GPU Rendering. Journal of Computer Graphics Techniques Hash Functions for GPU Rendering, 9(3), 20–38. http://jcgt.orghttp//jcgt.org%Ahttp://www.jcgt.org/published/0009/03/02/
uint3 pcg3d(uint3 v)
{
	v = v * 1664525 + 1013904223;
	v.x += v.y*v.z; v.y += v.z*v.x;
	v.z += v.x*v.y;
	v = pow(v.x,  v.x >> 16u);
	v.x += v.y*v.z; v.y += v.z*v.x;
	v.z += v.x*v.y;
	return v;
}

//
// Helper functions for noise
//
float easeIn(float interpolator) {
	return interpolator * interpolator;
}

float easeOut(float interpolator) {
	return 1 - easeIn(1 - interpolator);
}

float easeInOut(float interpolator) {
	float easeInValue = easeIn(interpolator);
	float easeOutValue = easeOut(interpolator);
	return lerp(easeInValue, easeOutValue, interpolator);
}
// 
// End Helper funciton for noise
//

//
// Noise function dirived from perlin noise 
//
float ValueNoise3d(float3 input) {
	float3 fraction = frac(input);

	float interpolatorX = easeInOut(fraction.x);
	float interpolatorY = easeInOut(fraction.y);
	float interpolatorZ = easeInOut(fraction.z);

	// Loop thought the cells to get a average gradient of a area
	float cellNoiseZ[2];
	[unroll]
	for (int z = 0; z <= 1; z++) {
		float cellNoiseY[2];
		[unroll]
		for (int y = 0; y <= 1; y++) {
			float cellNoiseX[2];
			[unroll]
			for (int x = 0; x <= 1; x++) {
				float3 cell = floor(input) + float3(x, y, z);
				float3 cellDirection = pcg3d(cell) * 2 - 1;
				float3 compareVector = fraction - float3(x, y, z);
				cellNoiseX[x] = dot(normalize(cellDirection), normalize(compareVector));
			}
			cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
		}
		cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
	}
	float noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);
	return noise;
}

float3 normal(int index, float3 p)
{
	// A quick look up table to get the normal requried
	if (TYPE_OF_NORMAL == 1)
	{
		//tetrahedron effect
		//const float h = 0.0001; // replace by an appropriate value
		const float2 k = float2(1, -1);
		return normalize(
			k.xyy * SDF(index, p + k.xyy * _NormalApproxStep) +
			k.yyx * SDF(index, p + k.yyx * _NormalApproxStep) +
			k.yxy * SDF(index, p + k.yxy * _NormalApproxStep) +
			k.xxx * SDF(index, p + k.xxx * _NormalApproxStep)
		);
	}
	else if (TYPE_OF_NORMAL == 2)
	{
		// forward and central distances unoptomised
		float3 h = float3(_NormalApproxStep, -_NormalApproxStep, 0);
		float regularSDF = SDF(index, p);
		return normalize(float3(
			SDF(index, p + h.xzz) - SDF(index, p - h.xzz),
			SDF(index, p + h.zxz) - SDF(index, p - h.zxz),
			SDF(index, p + h.zzx) - SDF(index, p - h.zzx)
			));
	}
	else // Type of normal zero will also go here
	{
		// forward and centeral differences optimised for efficiency over quality 
		float3 h = float3(_NormalApproxStep, -_NormalApproxStep, 0);
		float regularSDF = SDF(index, p);
		return normalize(float3(
			SDF(index, p + h.xzz) - regularSDF,
			SDF(index, p + h.zxz) - regularSDF,
			SDF(index, p + h.zzx) - regularSDF
			));
	}
}

bool intersect(float3 rPos, float3 rDir, float3 aabbMax, float3 aabbMin, out float t0, out float t1)
{
	float3 invR = 1.0 / rDir;     // r 中的所有值取倒数 -> (方向向量取倒数)
	float3 tbot = invR * (aabbMin - rPos);   // 上面所有值乘下面min和origin的差值   新的向量
	float3 ttop = invR * (aabbMax - rPos);   // 上面所有值乘下面max和origin的差值   新的向量
	float3 tmin = min(ttop, tbot);   // 取两个新向量中的最小值
	float3 tmax = max(ttop, tbot);   // 取两个新向量中的最大值
	float2 t = max(tmin.xx, tmin.yz);
	t0 = max(t.x, t.y);
	t = min(tmax.xx, tmax.yz);
	t1 = min(t.x, t.y);
	aabbMin = t0 <= t1 ? aabbMin : aabbMax;
	aabbMax = t0 <= t1 ? aabbMax : aabbMin;
	return t0 <= t1;
}

//TODO replace this with a actual version
float4 blend(float4 bg, float4 fg) 
{
	float forgroundAlphaCorrected = (fg.a * 0.1);
	bg.rgb *= (1 - _NewColorIntensity);
	fg.rgb *= (1 + _NewColorIntensity);

	float4 output = float4(0, 0, 0, 1 - (1 - fg.a) * (1 - bg.a));

	output.r = fg.r * fg.a / output.a + bg.r * bg.a * (1 - fg.a) / output.a;
	output.g = fg.g * fg.a / output.a + bg.g * bg.a * (1 - fg.a) / output.a;
	output.b = fg.b * fg.a / output.a + bg.b * bg.a * (1 - fg.a) / output.a;

	// code to clamp the values
	float m = sqrt((output.r * output.r) + (output.g * output.g) + (output.b * output.b));
	output.rgb = (m == 0) || (_ColorThreshold > m) ? output.rgb : output.rgb * (_ColorThreshold / m);

	return (output.a < 1.0e-6) ? fg : output;
}

//TODO replace this with a actual version
float4 blend(float4 bg, float4 fg, float DistancePercentage)
{
	float forgroundAlphaCorrected = (fg.a  * 0.1);
	float4 output = float4(0, 0, 0, (1 - (1 - (forgroundAlphaCorrected * DistancePercentage)) * (1 - bg.a)));
	
	output.r = (fg.r * DistancePercentage) * forgroundAlphaCorrected / output.a + bg.r * bg.a * (1 - (fg.a * DistancePercentage)) / output.a;
	output.g = (fg.g * DistancePercentage) * forgroundAlphaCorrected / output.a + bg.g * bg.a * (1 - (fg.a * DistancePercentage)) / output.a;
	output.b = (fg.b * DistancePercentage) * forgroundAlphaCorrected / output.a + bg.b * bg.a * (1 - (fg.a * DistancePercentage)) / output.a;

	// code to clamp the values
	float m = sqrt((output.r * output.r) + (output.g * output.g) + (output.b * output.b));
	output.rgb = (m == 0) || (_ColorThreshold > m) ? output.rgb : output.rgb * (_ColorThreshold / m);

	return output;
}

float3 DistanceBasedIntencity(float tnear, float tfar, float t, float3 voxelColor)
{
	float percentThough =  1 + ((lerp(tnear, tfar, t) - 0.5) * 0.25);
	return percentThough > 0 ? voxelColor * percentThough : voxelColor / abs(percentThough);
}

float cacluateNextStep(float t, float4 currentColor, float distance)
{
	float d = abs(distance);
	return t - (currentColor == float4(0, 0, 0, 0) ? d : _MinDistanceForStep);
}

int determineBestIndexFound(bool withinRange, bool MostImportant, int currentIndex, int bestIndexFound)
{
	return withinRange && MostImportant ? currentIndex : bestIndexFound;
}

float4 blendingLogic(float4 orginalColor, float4 newColor, bool mostImportant, bool withinRange) 
{
	return orginalColor == float4(0, 0, 0, 0) || mostImportant && withinRange ? newColor : blend(orginalColor, newColor);
}

v2f vert(appdata v)
{
	v2f o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));//ObjSpaceCameraPos();
	o.worldSpacePos = mul(unity_ObjectToWorld, v.vertex);
	o.rayDirection = v.vertex - o.rayOrigin;
	o.vertex = v.vertex;
	o.local = v.vertex;

	return o;
}