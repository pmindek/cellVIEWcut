Shader "Custom/Handle" 
{
	SubShader
	{
		Pass
		{
			ZTest  Always
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			

			uniform float _HandleSize;
			uniform float4 _HandleColor;
			uniform float4x4 _ModelMatrix;

			float4 vert(appdata_base v) : POSITION
			{
				//float4 temp = mul(_ModelMatrix, v.vertex);
				return mul(UNITY_MATRIX_MVP, v.vertex);
			}

				float4 frag(float4 sp:VPOS) : COLOR
			{
				return _HandleColor;
			}

			ENDCG
		}
	}
}