Shader "Custom/Handle" 
{
	SubShader
	{
		Tags{ "Queue" = "Overlay" }

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Always

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"
		
			uniform float4 _HandleColor;

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v) 
			{
				v2f output;
				output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return output;
			}

			float4 frag(v2f input) : COLOR
			{
				return _HandleColor;
			}

			ENDCG
		}

		Pass
		{
			ZWrite Off
			ZTest Always

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"

			uniform float4 _HandleColor;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 viewNormal : FLOAT30;
			};

			v2f vert(appdata_base v)
			{
				v2f output;

				output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				output.viewNormal = normalize(mul(UNITY_MATRIX_MV, float4(v.normal, 0.0)).xyz);
				return output;
			}

			float4 frag(v2f input) : COLOR
			{
				float ndotl = max(dot(input.viewNormal, float3(0,0,1)), 0.1) * 1.25;
				return _HandleColor * ndotl;
			}

			ENDCG
		}
	}
}