Shader "Unlit/HatchingSurfaceShader"
{
    Properties
    {
        _Volume("Volume", 3D) = "" {}
        _NormalMap("Normal Map", 3D) = "" {}
        _Threshold("Threshold", Range(0.0, 1.0)) = 0.95
        _min_Range("min_Range", Range(0.0, 1.0)) = 0.01
        _max_Range("max_Range", Range(0.0, 1.0)) = 0.99
        _EdgeThreshold("edge threshold", Range(0.0, 1.0)) = 0.5
        _LightDirection("Light Direction", Vector) = (0.0, 0.0, 1.0, 0.0)
        _ViewDirection("View Direction", Vector) = (0.0, 0.0, 1.0, 0.0)
        _Range("Minimal Tolerance For Edge Based", Range(0.0, 1.0)) = 0.5
        _StepsToEffect("Amount Of Steps to Effect", Range(0, 10)) = 0.5
        _EffectColor("Color", Color) = (1, 1, 1, 1)

		_GridSpaceCenter("Center of Each Grid", Vector) = (0.0, 0.0, 0.0, 0.0)
		_GridSpaceDimentions("Grid Space Dimentions", Vector) = (0.0, 0.0, 0.0, 0.0)
		_GridDimentions("Grid Dimentions", Vector) = (0.0, 0.0, 0.0, 0.0)
    }
		CGINCLUDE
			ENDCG

			SubShader{
				Cull Back
				Blend SrcAlpha OneMinusSrcAlpha
				ZTest Always


				Pass
				{
					CGPROGRAM

					#ifndef __VOLUME_RENDERING_INCLUDED__
					#define __VOLUME_RENDERING_INCLUDED__

					#include "UnityCG.cginc"

					#ifndef ITERATIONS
					#define ITERATIONS 256
					#endif


					#ifndef AmountOfSegments
					#define AmountOfSegments 6
					#endif

					#ifndef AMOUNT_OF_LAYERS
					#define AMOUNT_OF_LAYERS 1
					#endif

					#pragma exclude_renderers d3d11_9x

					// Variables
					half4 _Colors[AmountOfSegments];
					float _Density[AmountOfSegments];
					float _Intensity[AmountOfSegments];
					sampler3D _Volume;
					sampler3D _NormalMap;
					half _Threshold;
					half _min_Range, _max_Range;
					half3 _SliceMin, _SliceMax;
					float4x4 _AxisRotationMatrix;
					float3 aabbMin, aabbMax;
					half _EdgeThreshold;
					float _MaxDimention;
					float _Range;
					float _StepsToEffect;
					float4 _EffectColor;


					// Hatching Details
					float4 _GridSpaceCenter;
					float3 _GridSpaceDimentions;
					float3 _GridDimentions;
					float4x4 _GridDirectionMatrix; // allows me to set the position of the hashing exactly
					float4x4 _GridDirectionMatrixArray[AMOUNT_OF_LAYERS];
					float _HatchingTaperDivsor;

					/**
					存储了每个点的坐标以及每个点到摄像机的方向向量
					stored the coordinates of each point under the object space and the direction vector from each point to camera
					*/
					struct Ray {
						float3 origin;    // 模型空间的xyz坐标
						float3 dir;		  // 世界空间下点到摄像机的方向向量转模型空间
					};

					// min 和max均是两个常量，猜测为一个区间
					struct AABB {
						float3 min;
						float3 max;
					};

					// https://iquilezles.org/articles/distfunctions/
					float sdBox(float3 p, float3 c, float3 b)
					{
						float3 q = abs(p - c) - b;
						return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
					}

					float SDFHashing(float3 p, float multiplier)
					{
						float3 correctGD = _GridDimentions.xyz * multiplier;

						// Transform the current postito the grid space _GridDirectionMatrix
						p = ((abs(mul(_GridDirectionMatrixArray[0], p)) % correctGD) - (correctGD / 2.0));

						// work out where this box sits in this grid
						return sdBox(p, _GridSpaceCenter.xyz * multiplier, correctGD);
					}

					float SDFHashing(float3 p, float multiplier, int layerIndex, float dotp)
					{
						float3 correctGD = _GridDimentions.xyz * (multiplier);

						// Transform the current postito the grid space _GridDirectionMatrix
						p = ((abs(mul(_GridDirectionMatrixArray[layerIndex], p)) % correctGD) - (correctGD / 2.0));

						//(abs(dotp) / (1 + _Importance[currentIndex]) - _HatchingTaperDivsor)

						// work out where this box sits in this grid
						return sdBox(p, _GridSpaceCenter.xyz * multiplier, _GridSpaceDimentions.xyz * multiplier);
					}


					// tnear -> t0
					// tfar -> t1
					bool intersect(Ray r, AABB aabb, out float t0, out float t1)
					{
						float3 invR = 1.0 / r.dir;     // r 中的所有值取倒数 -> (方向向量取倒数)
						float3 tbot = invR * (aabb.min - r.origin);   // 上面所有值乘下面min和origin的差值   新的向量
						float3 ttop = invR * (aabb.max - r.origin);   // 上面所有值乘下面max和origin的差值   新的向量
						float3 tmin = min(ttop, tbot);   // 取两个新向量中的最小值
						float3 tmax = max(ttop, tbot);   // 取两个新向量中的最大值
						float2 t = max(tmin.xx, tmin.yz);
						t0 = max(t.x, t.y);
						t = min(tmax.xx, tmax.yz);
						t1 = min(t.x, t.y);
						return t0 <= t1;
					}


					// 世界空间坐标转模型空间
					float3 localize(float3 p) {
						return mul(unity_WorldToObject, float4(p, 1)).xyz;
					}

					float3 get_uv(float3 p) {
						//float3 local = localize(p);
						return (p + 0.5);
					}


					// 输出值要么是v，要么是0
					float sample_volume(float tex, float3 p)
					{
						//
						float v = tex;

						// work out were the matrix rotation should be
						float3 axis = mul(_AxisRotationMatrix, float4(p, 0)).xyz;
						axis = get_uv(axis);

						// make sure this object should remain visible
						float min = step(_SliceMin.x, axis.x) * step(_SliceMin.y, axis.y) * step(_SliceMin.z, axis.z);
						float max = step(axis.x, _SliceMax.x) * step(axis.y, _SliceMax.y) * step(axis.z, _SliceMax.z);

						return v * min * max;
					}



					bool outside(float3 uv) {
						const float EPSILON = 0.01;
						float lower = -EPSILON;
						float upper = 1 + EPSILON;
						return (
								  uv.x < lower || uv.y < lower || uv.z < lower || uv.x > upper || uv.y > upper || uv.z > upper
							  );
					}

					float4 map(int index, float density) {
						// return the part of the color between the two colors
						float4 color = lerp(_Colors[index], _Colors[index - 1], (density - _Density[index - 1]) / (_Density[index] - _Density[index - 1]));

						return color;
					}

					float4 map(float density) {
						//work out what color to use by finding the correct index
						int index = 0;

						[unroll]
						for (int iter = 0; iter < AmountOfSegments; iter++) {
							// increment the index if correct
							index += (density > _Density[iter]);
						}

						// return the part of the color between the two colors
						return lerp(_Colors[index], _Colors[index - 1], (density - _Density[index - 1]) / (_Density[index] - _Density[index - 1]));
					}

					struct appdata {
						float4 vertex : POSITION;
						float2 uv : TEXCOORD0;    //模型纹理，，，这玩意儿居然还有纹理？
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						float2 uv : TEXCOORD0;
						float3 world : TEXCOORD1;   // 世界空间坐标值
						float3 local : TEXCOORD2;   // 模型空间坐标的xyz值
						UNITY_VERTEX_OUTPUT_STEREO
					};

					// calculate the color's luminance
					fixed luminance(fixed3 color) {
						return 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
					}

					float3 Slerp(float3 start, float3 end, float percent) {
						float dot = start.x * end.x + start.y * end.y + start.z * end.z;

						float min = step(-1, dot);
						float max = step(dot, 1);

						dot = lerp(-1, dot, min);
						dot = lerp(dot, 1, max);

						float theta = degrees(acos(dot)) * percent;
						float3 relativeVec = end - start * dot;
						relativeVec = normalize(relativeVec);

						return (start * cos(theta) + relativeVec * sin(theta));
					}

					// using this function to determine the edge 
					half sobelEdgeDetection(float3 positionList[9]) {

						const half Gx[9] = {-1, 0, 1,
											-2, 0, 2,
											-1, 0, 1};
						const half Gy[9] = {-1, -2, -1,
											0, 0, 0,
											1, 2, 1};


						/*
						const half Gx[5] = {-1, 1,
											0, -1, 1};
						const half Gy[5] = {-1, -1, 0,
											1, 1};
						*/

						half edgeX = 0;
						half edgeY = 0;

						[unroll]
						for (int i = 0; i < 9; i++) {
							//fixed temoColor = luminance(tex3D(_Volume, positionList[i]).rgb);
							fixed tempAlgpa = tex3D(_Volume, positionList[i]).a;
							edgeX += tempAlgpa * Gx[i];
							edgeY += tempAlgpa * Gy[i];
						}

						half edge = 1 - abs(edgeX) - abs(edgeY);
						return edge;
					}

					float4 BlendColor(float4 fg, float4 bg) {
						float4 output = 1 - (1 - fg.a) * (1 - bg.a);
						//if (output.a < 1.0e-6) return fg; // Fully transparent -- R,G,B not important
						output.r = fg.r * fg.a / output.a + bg.r * bg.a * (1 - fg.a) / output.a;
						output.g = fg.g * fg.a / output.a + bg.g * bg.a * (1 - fg.a) / output.a;
						output.b = fg.b * fg.a / output.a + bg.b * bg.a * (1 - fg.a) / output.a;

						return (output.a < 1.0e-6) ? bg : output;
					}

					// gets the index of the current values values are stored at.
					float4 GetIndex(float density) {
						//work out what color to use by finding the correct index
						int index = 0;

						[unroll]
						for (int iter = 0; iter < AmountOfSegments; iter++)
						{
							// increment the index if correct
							index += (density > _Density[iter]);
						}

						return index;
					}

					float3 postionProcess(v2f f, half x, half y) {
						float3 viewPos = mul((float3x3) UNITY_MATRIX_V, f.world);
						viewPos.x += x;
						viewPos.y += y;
						float3 pos = mul((float3x3)UNITY_MATRIX_I_V, viewPos);
						float3 dir = (pos - _WorldSpaceCameraPos);
						return normalize(mul(unity_WorldToObject, dir));
					}

					void Unity_Saturation_float(float3 In, float Saturation, out float3 Out)
					{
						float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
						Out = luma.xxx + Saturation.xxx * (In - luma.xxx);
					}

					v2f vert(appdata v) {

						v2f o;

						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_OUTPUT(v2f, o);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


						o.vertex = UnityObjectToClipPos(v.vertex);
						o.uv = v.uv;
						o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
						o.local = v.vertex.xyz;
						return o;
					}

					fixed4 frag(v2f i) : SV_Target {

						fixed4 dstList[5];

						Ray ray;
						// ray.origin = localize(i.world);
						ray.origin = i.local;

						ray.dir = postionProcess(i, 0, 0);

						AABB aabb;
						aabb.min = float3(-0.5, -0.5, -0.5);
						aabb.max = float3(0.5, 0.5, 0.5);

						float tnear;
						float tfar;
						intersect(ray, aabb, tnear, tfar);

						tnear = max(0.0, tnear); // 让tnear恒大于等于0

						float3 start = ray.origin + ray.dir * tnear;
						float3 end = ray.origin + ray.dir * tfar;

						float maxDist = length(float3(-0.5, -0.5, -0.5) - float3(0.5, 0.5, 0.5)); // approx: 1.732

						float dist = abs(tfar - tnear); // float dist = distance(start, end);
						float step_size = dist / float(ITERATIONS);
						float3 ds = normalize(end - start) * step_size;

						float4 dst = float4(0, 0, 0, 0);
						float3 p = start;

						int touched = 0;

						[loop]
						for (int iter = 0; iter < ITERATIONS; iter++)
						{
							// convert the postions back to uv's
							float3 uv = get_uv(p);   //p中每个值加0.5

							// mapping the color
							float4 tex = tex3D(_Volume, uv);
							float currentValue = tex.a;

							// detemine if this is the first hit from the ray
							bool wasTouched = (currentValue > _Range);
							touched += wasTouched;

							float v = sample_volume(currentValue, p);
							//normalize(mul(unity_WorldToObject, dir))
							float normal = dot(normalize((p - aabb.max)), tex3D(_NormalMap, uv).rgb);

							float hatching = SDFHashing(p, 1, 0, normal);

							// calcuate the current result into a byte size integer
							float currentDensity = v * 256;

							float inRange = (v > _min_Range) * (v < _max_Range) * (currentDensity > _Density[1]);

							// get the apprriate color index
							float index = GetIndex(currentDensity);

							// create the new color
							float4 src = map(index, currentDensity * inRange);

        src = wasTouched && (touched < _StepsToEffect && hatching > 0) ? float4(abs(_EffectColor).rgb, abs(_EffectColor).a * (-normal / 2)) : src;
							//src = wasTouched && (touched < _StepsToEffect) ? abs(_EffectColor) * abs(normal) : src;

							float power = _Intensity[index];

							dst = BlendColor(src * power, dst);

							///
							/// Color End
							///
							p += max(step_size, tex.r * maxDist) * ray.dir;

							// stop if we we are ove the color
							if (dst.a > _Threshold) {
								break;
							}

						}
					return dst;

				}
				#endif 
				#pragma vertex vert
				#pragma fragment frag

				ENDCG

			}
		}
}
