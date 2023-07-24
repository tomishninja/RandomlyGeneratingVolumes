Shader "Custom/SpectrumShader"
{
	Properties
	{
		_MainTex("RGB Spectrum Texture", 3D) = "white" {}
		_StepSize("Step Size", Range(0.001, 0.1)) = 0.01
		_Density("Density", Range(0.01, 1.0)) = 0.1

		_CuttingPlane1Position("Cutting Plane 1 Position", Vector) = (0, 0, 0, 0)
		_CuttingPlane1Normal("Cutting Plane 1 Normal", Vector) = (0, 0, 0, 0)

		_CuttingPlane2Position("Cutting Plane 2 Position", Vector) = (0, 0, 0, 0)
		_CuttingPlane2Normal("Cutting Plane 2 Normal", Vector) = (0, 0, 0, 0)

		_CuttingPlane3Position("Cutting Plane 3 Position", Vector) = (0, 0, 0, 0)
		_CuttingPlane3Normal("Cutting Plane 3 Normal", Vector) = (0, 0, 0, 0)
	}

		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Lambert alpha

			sampler3D _MainTex;
			float _StepSize;
			float _Density;

			float4 _CuttingPlane1Position;
			float4 _CuttingPlane1Normal;
			float4 _CuttingPlane2Position;
			float4 _CuttingPlane2Normal;
			float4 _CuttingPlane3Position;
			float4 _CuttingPlane3Normal;

			struct Input
			{
				float3 worldPos;
			};

			bool IsClipped(float3 position)
			{
				float4 clipPlane1 = float4(_CuttingPlane1Normal.xyz, dot(_CuttingPlane1Position.xyz, _CuttingPlane1Normal.xyz));
				float4 clipPlane2 = float4(_CuttingPlane2Normal.xyz, dot(_CuttingPlane2Position.xyz, _CuttingPlane2Normal.xyz));
				float4 clipPlane3 = float4(_CuttingPlane3Normal.xyz, dot(_CuttingPlane3Position.xyz, _CuttingPlane3Normal.xyz));

				// Check clipping with each plane
				if (dot(position, clipPlane1.xyz) > clipPlane1.w ||
					dot(position, clipPlane2.xyz) > clipPlane2.w ||
					dot(position, clipPlane3.xyz) > clipPlane3.w)
				{
					// Ray is clipped
					return true;
				}

				return false;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				float4 color = float4(0, 0, 0, 0);
				float3 position = IN.worldPos;
				float t = 0.0;

				// Raymarching loop
				for (int i = 0; i < 100; i++) // Maximum iterations
				{
					// Check clipping with cutting planes
					if (IsClipped(position))
						break;

					// Sample the color from the 3D texture at the current position
					float4 texColor = tex3D(_MainTex, position);

					// Accumulate color with density
					color += texColor * _Density;

					// Move along the ray
					position += _StepSize * normalize(texColor.rgb);
					t += _StepSize;

					// Check for early termination
					if (t > 1.0 || color.a >= 0.99)
						break;
				}

				o.Albedo = color.rgb;
				o.Alpha = color.a;
			}
			ENDCG
		}
}
