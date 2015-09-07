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
		
			uniform float4 _HandleColor;
			uniform int _EnableShading;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 viewNormal : FLOAT30;
				int doShading : INT;
			};

			v2f vert(appdata_base v) 
			{
				v2f output;

				output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				output.viewNormal = normalize(mul(UNITY_MATRIX_MV, float4(v.normal, 0.0)).xyz);
				output.doShading = v.normal != float3(0, 0, 0);
				return output;
			}

			float4 frag(v2f input) : COLOR
			{
				float ndotl = (_EnableShading == 1) ? max(dot(input.viewNormal, float3(0,0,1)), 0.1) : 1;
				return _HandleColor * ndotl;
			}

			ENDCG
		}
	}
}