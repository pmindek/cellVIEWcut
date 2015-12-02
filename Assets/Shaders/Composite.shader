Shader "Custom/Composite" 
{
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		// *****
		Pass
		{
			ZWrite On
			ZTest Less

			CGPROGRAM
#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			#include "UnityCG.cginc"

			sampler2D _ColorTexture;
			sampler2D_float _DepthTexture;

			void frag(v2f_img i,  out float4 color : COLOR, out float depth : DEPTH)
			{
				color = tex2D(_ColorTexture, i.uv);
				depth = tex2D(_DepthTexture, i.uv);
			}
			ENDCG
		}

		// *****
		Pass
		{
			ZWrite On
			ZTest Always

			CGPROGRAM
#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			sampler2D_float _DepthTexture;
			sampler2D_float _CameraDepthTexture;

			#include "UnityCG.cginc"
			
			void frag(v2f_img i, out float depth : DEPTH)
			{
				float customDepth = tex2D(_DepthTexture, i.uv);
				float cameraDepth = tex2D(_CameraDepthTexture, i.uv);
				depth = customDepth < cameraDepth ? customDepth : cameraDepth;
			}
			ENDCG
		}

		// *****
		Pass
		{
			ZWrite On
			ZTest Always

			CGPROGRAM
#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			sampler2D_float _DepthTexture;
			sampler2D_float _CameraDepthTexture;
			sampler2D_float _CameraDepthNormalsTexture;

			#include "UnityCG.cginc"

			void frag(v2f_img i, out float4 depthNormals : COLOR)
			{
				float customDepth = tex2D(_DepthTexture, i.uv);
				float cameraDepth = tex2D(_CameraDepthTexture, i.uv);
				depthNormals = customDepth < cameraDepth ? float4(0.48,0.52,0,0) : tex2D(_CameraDepthNormalsTexture, i.uv);
			}
			ENDCG
		}

		Pass 
		{
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag	
					
			#pragma target 5.0	
			#pragma only_renderers d3d11	

            #include "UnityCG.cginc"
			#include "Helper.cginc"	

			uniform Texture2D<int> _IdTexture; 
			StructuredBuffer<float4> _ProteinColors;
			StructuredBuffer<float4> _ProteinInstanceInfo;

			StructuredBuffer<float4> _LipidInstanceInfo;

            void frag(v2f_img i, out float4 color : COLOR0) 
			{   
				int2 uv = i.uv * _ScreenParams.xy; 
				int id = _IdTexture[uv];

				//if(id == -3)
				//{
				//	color = float4(1,0,0,0);
				//}
				//else if(id == -2)
				//{
				//	color = float4(1,240.0/255.0,114.0/255.0,0);
				//}				
				
				if(id >= 0)
				{
					// if is lipid
					if(id >= 100000)
					{		
						float4 lipidInfo = _LipidInstanceInfo[id - 100000];		
						if(lipidInfo.x > 43) color = float4(1,1,1,1);
						else color = float4(0,1,1,1);
						//color = float4(0,1,1,1);
					}
					else
					{
						float4 proteinInfo = _ProteinInstanceInfo[id];
						float4 proteinColor = _ProteinColors[proteinInfo.x];

						float diffuse = proteinInfo.z;
						//color = float4(ColorCorrection(proteinColor.xyz) * diffuse, 1);	
						color = float4(ColorCorrection(proteinColor.xyz), 1);	
					}						
				}
				else
				{
					discard;
				}
            }
            
            ENDCG
        }

		Pass
		{
			ZTest Always

			CGPROGRAM
			#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			sampler2D _MainTex;

			#include "UnityCG.cginc"

			void frag(v2f_img i, out float4 color : COLOR)
			{
				float4 c = tex2D(_MainTex, i.uv);
				if(c.z > 0) color = float4(c.z/50,0,0,0);
				else color = float4(0,0,0,0);

				//if(c.z < 0) color = float4(1,0,0,1);
				//else color = float4(0,0,1,1);
			}
			ENDCG
		}
	}	

	FallBack "Diffuse"
}