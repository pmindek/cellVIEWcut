Shader "Custom/CutObjectShader" 
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
		
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;			

			struct appdata 
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = float4(v.texcoord.xy, 0, 0);
				return o;
			}

			float4 frag(v2f input) : SV_Target
			{
				return float4(tex2D(_MainTex, input.uv * 10).xyz, 0.25);
			}

		ENDCG
		}		
	}

	Fallback "Diffuse"
}