Shader "Custom/Composite" 
{
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		Pass 
		{
			ZWrite On
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;

            void frag(v2f_img i,  out float4 color : COLOR0, out float depth : DEPTH) 
			{             
				color = tex2D(_MainTex, i.uv);       
				depth = tex2D(_CameraDepthTexture, i.uv);
            }
            ENDCG
        }

		Pass 
		{
			ZWrite On
			ZTest Lequal

            CGPROGRAM
            
			
			#pragma only_renderers d3d11
			#pragma target 5.0		

			#pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;

			sampler2D _ColorTexture;
			sampler2D_float _DepthTexture;

            void frag(v2f_img i,  out float4 color : COLOR0, out float depth : depth) 
			{   
				float customDepth = (tex2D(_DepthTexture, i.uv));  		
				float cameraDepth = (tex2D(_CameraDepthTexture, i.uv));  	
					
				bool depthTest = customDepth < cameraDepth;
				color = depthTest ? tex2D(_ColorTexture, i.uv) : tex2D(_MainTex, i.uv);
				depth = depthTest ? customDepth : cameraDepth;
				    
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

            #include "UnityCG.cginc"
			
			Texture3D<float> _HiZMap;

            void frag(v2f_img i, out float4 color : COLOR0) 
			{       
				uint2 coord = uint2(i.uv.x * _ScreenParams.x, i.uv.y * _ScreenParams.y);   
				color.r = Linear01Depth(_HiZMap[uint3(coord, 4)]); 
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

            #include "UnityCG.cginc"
			#include "Helper.cginc"	

			uniform Texture2D<int> _IdTexture; 
			StructuredBuffer<float4> _ProteinColors;
			StructuredBuffer<float4> _ProteinInstanceInfo;

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
					float4 proteinInfo = _ProteinInstanceInfo[id];
					float4 proteinColor = _ProteinColors[proteinInfo.x];
					color = float4(ColorCorrection(proteinColor.xyz), 1);
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
            #pragma vertex vert_img
            #pragma fragment frag			
			#pragma target 5.0		

            #include "UnityCG.cginc"
			
			sampler2D_float _DepthTexture;

            void frag(v2f_img i, out float4 color : COLOR0) 
			{       
				//uint2 coord = uint2(i.uv.x * _ScreenParams.x, i.uv.y * _ScreenParams.y);   
				color.r = Linear01Depth(tex2D(_DepthTexture, i.uv)); 
            }
            
            ENDCG
        }
	}	

	FallBack "Diffuse"
}