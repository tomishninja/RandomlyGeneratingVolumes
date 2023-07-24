Shader "Unlit/HatchingTwentyThreeElements"
{
	Properties
	{
		_EffectColor("Color of Hatching", Color) = (1.0, 1.0, 1.0)

		_GridSpaceCenter("Place Of Empty Box(xyz)", Vector) = (0,0,0,0)
		_GridSpaceDimentions("Empty Box Dimentions (xyz)", Vector) = (0,0,0,0)
		_GridDimentions("Grid Dimentions (xyz)", Vector) = (0,0,0,0)
		_HatchingTaperDivsor("Divisor for the rate of tapering", Float) = 5

		_Tolerance("Tolerance", Float) = 0.01
		_MaxDistance("Max. Distance", Float) = 1000.0
		_MaxSteps("Max. Steps", Int) = 100
		_MinDistanceForStep("Min Distance For Step", Float) = 0.01
		_NewColorIntensity("Color Intensity for Adding Colors", Float) = 0.025
		_NormalApproxStep("Normal Approx. Step", Float) = 0.001
		_Resolution("Square Resolution", Int) = 100
		_ResulitionForGradient("Cords to User for Noise Gradient", Float) = 8
		_NoiseSuppression("Noise Multipler", Range(1.0, 5.0)) = 1.0
		_ColorThreshold("Threshold of The Output Color", Range(0.0, 4.0)) = 1.41

		_HashLineA("Hash Line A", Vector) = (127.1,311.7, 74.7, 1)
		_HashLineB("Hash Line B", Vector) = (269.5,183.3,246.1, 1)
		_HashLineC("Hash Line C", Vector) = (113.5,271.9,124.6, 1)
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" }
		LOD 100

		Pass
		{

			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#ifndef AMOUNT_OF_COLORS
			#define AMOUNT_OF_COLORS 23
			#endif

			#include "UnityCG.cginc"
			#include "Assets/Scripts/Shaders/Examples/SDFVolumeGeneralFunctions.cginc"
			#include "Hatching.cginc"

			ENDCG
		}
	}
}
