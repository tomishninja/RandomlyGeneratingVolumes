Shader "Unlit/VolumeRendering"
{
	Properties
	{
		_Volume("Volume", 3D) = "" {}
		_Threshold("Threshold", Range(0.0, 1.0)) = 0.95
		_min_Range("min_Range", Range(0.0, 1.0)) = 0.01
		_max_Range("max_Range", Range(0.0, 1.0)) = 0.99
		_SliceMin("Slice min", Vector) = (0.0, 0.0, 0.0, -1.0)
		_SliceMax("Slice max", Vector) = (1.0, 1.0, 1.0, -1.0)
	}

			SubShader{
				Tags { "Queue" = "Transparent" }
				LOD 100

				Pass
				{
					Blend SrcAlpha OneMinusSrcAlpha
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

					sampler3D _Volume;
					half _Threshold;
					half _min_Range, _max_Range;
					half3 _SliceMin, _SliceMax;
					float4x4 _AxisRotationMatrix;

					struct Ray {
						float3 origin;
						float3 dir;
					};

					struct AABB {
						float3 min;
						float3 max;
					};

					bool intersect(Ray r, AABB aabb, out float t0, out float t1)
					{
						float3 invR = 1.0 / r.dir;
						float3 tbot = invR * (aabb.min - r.origin);
						float3 ttop = invR * (aabb.max - r.origin);
						float3 tmin = min(ttop, tbot);
						float3 tmax = max(ttop, tbot);
						float2 t = max(tmin.xx, tmin.yz);
						t0 = max(t.x, t.y);
						t = min(tmax.xx, tmax.yz);
						t1 = min(t.x, t.y);
						return t0 <= t1;
					}

					float3 localize(float3 p) {
						return mul(unity_WorldToObject, float4(p, 1)).xyz;
					}

					float3 get_uv(float3 p) {
						//float3 local = localize(p);
						return (p + 0.5);
					}

					float4 sample_volume(float3 uv, float3 p)
					{
						// texture
						float4 tex = tex3D(_Volume, uv);

						// work out were the matrix rotation should be
						float3 axis = mul(_AxisRotationMatrix, float4(p, 0)).xyz;
						axis = get_uv(axis);

						float max = step(axis.x, _SliceMax.x) + step(axis.y, _SliceMax.y) + step(axis.z, _SliceMax.z);

						return (max < 1) ? 0 : tex;
					}

					  bool outside(float3 uv)
					  {
						const float EPSILON = 0.01;
						float lower = -EPSILON;
						float upper = 1 + EPSILON;
						return (
								  uv.x < lower || uv.y < lower || uv.z < lower || uv.x > upper || uv.y > upper || uv.z > upper
							  );
					  }

					  struct appdata
					  {
						float4 vertex : POSITION;
						float2 uv : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					  };

					  struct v2f
					  {
						float4 vertex : SV_POSITION;
						float2 uv : TEXCOORD0;
						float3 world : TEXCOORD1;
						float3 local : TEXCOORD2;
						UNITY_VERTEX_OUTPUT_STEREO
					  };

					  v2f vert(appdata v)
					  {
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

					  fixed4 frag(v2f i) : SV_Target
					  {
						Ray ray;
					  // ray.origin = localize(i.world);
					  ray.origin = i.local;

					  // world space direction to object space
					  float3 dir = (i.world - _WorldSpaceCameraPos);
					  ray.dir = normalize(mul(unity_WorldToObject, dir));

					  AABB aabb;
					  aabb.min = float3(-0.5, -0.5, -0.5);
					  aabb.max = float3(0.5, 0.5, 0.5);

					  float tnear;
					  float tfar;
					  intersect(ray, aabb, tnear, tfar);

					  tnear = max(0.0, tnear);

					  float3 start = ray.origin + ray.dir * tnear;
					  //float3 start = ray.origin;
					  float3 end = ray.origin + ray.dir * tfar;
					  float dist = abs(tfar - tnear); // float dist = distance(start, end);
					  float step_size = dist / float(ITERATIONS);
					  float3 ds = normalize(end - start) * step_size;

					  float4 dst = float4(0, 0, 0, 0);
					  float3 p = start;

					  [unroll]
					  for (int iter = 0; iter < ITERATIONS; iter++)
					  {
						  // get shader info
						  float3 uv = get_uv(p);

						  dst += sample_volume(uv, p);
						  p += ds;

						  // work out what the threashhold is
						  if (dst.a > _Threshold) break;
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