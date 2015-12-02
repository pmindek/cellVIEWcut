Shader "Custom/OcclusionQueries"
{
	CGINCLUDE

	#include "UnityCG.cginc"
	#include "Helper.cginc"		

	float _Scale;
	StructuredBuffer<float> _ProteinRadii;
	StructuredBuffer<float4> _ProteinInstanceInfo;
	StructuredBuffer<float4> _ProteinInstancePositions;
		
	StructuredBuffer<float4> _LipidInstanceInfo;
	StructuredBuffer<float4> _LipidInstancePositions;

	StructuredBuffer<int4> _OccludeeSphereBatches;
	RWStructuredBuffer<int> _FlagBuffer : register(u1);

	struct gs_input
	{
		int id : INT0;
		float4 sphere : FLOAT40;
	};

	struct fs_input
	{
		nointerpolation int id : INT0;
		nointerpolation float radius : FLOAT0;

		float2 uv: TEXCOORD0;
		centroid float4 pos : SV_Position;
	};

	//--------------------------------------------------------------------------------------

	void vs_protein(uint id : SV_VertexID, out gs_input output)
	{		
		int idx = _OccludeeSphereBatches[id].x;

		float4 infos = _ProteinInstanceInfo[idx];
		float radius = _ProteinRadii[infos.x] * _Scale * 1;
		float3 pos = _ProteinInstancePositions[idx].xyz * _Scale;
		
		output.id = idx;
		output.sphere = float4(pos, radius);
	}
	
	void vs_lipid(uint id : SV_VertexID, out gs_input output)
	{		
		int idx = _OccludeeSphereBatches[id].x;

		//float4 infos = _LipidInstanceInfo[idx];
		float4 sphere = _LipidInstancePositions[idx] * _Scale;
		
		output.id = idx;
		output.sphere = sphere;
	}

	//--------------------------------------------------------------------------------------

	[maxvertexcount(4)]
	void gs_sphere(point gs_input input[1], inout TriangleStream<fs_input> triangleStream)
	{
		// Discard unwanted atoms
		if (input[0].sphere.w <= 0) return;

		float4 viewPos = mul(UNITY_MATRIX_MV, float4(input[0].sphere.xyz, 1));
		viewPos -= normalize(viewPos) * input[0].sphere.w;
		float4 projPos = mul(UNITY_MATRIX_P, float4(viewPos.xyz, 1));
		float4 offset = mul(UNITY_MATRIX_P, float4(input[0].sphere.w, input[0].sphere.w, 0, 0));

		fs_input output;
		output.id = input[0].id;
		output.radius = input[0].sphere.w;

		output.uv = float2(1.0f, 1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(1.0f, -1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(-1.0f, 1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(-1.0f, -1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);
	}

	//--------------------------------------------------------------------------------------
	
	void fs_sphere(fs_input input, out float4 color: COLOR)
	{
		float lensqr = dot(input.uv, input.uv);
		if (lensqr > 1) discard;
		color = float4(1, 0, 0, 1);		
	}	

	[earlydepthstencil] // Necessary when writing to UAV's otherwise the depth stencil test will happen after the fragment shader
	void fs_sphere2(fs_input input)
	{	
		_FlagBuffer[input.id] = 1;
	}

	ENDCG		

	SubShader
	{
		Pass
		{
			ZWrite On
			ZTest Lequal

			// These stencil values will write 1 in the stencil channel for each instance drawn
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_protein			
			#pragma geometry gs_sphere			
			#pragma fragment fs_sphere

			ENDCG
		}

		Pass
		{
			ZWrite Off
			ZTest Less

			// These stencil values will discard instances drawn outiside of the mask
			Stencil
			{
				Ref 1
				Comp equal
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_protein			
			#pragma geometry gs_sphere		
			#pragma fragment fs_sphere2

			ENDCG
		}

		Pass
		{
			ZWrite On
			ZTest Lequal

			// These stencil values will write 1 in the stencil channel for each instance drawn
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_lipid			
			#pragma geometry gs_sphere			
			#pragma fragment fs_sphere

			ENDCG
		}

		Pass
		{
			ZWrite Off
			ZTest Less

			// These stencil values will discard instances drawn outiside of the mask
			Stencil
			{
				Ref 1
				Comp equal
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_lipid			
			#pragma geometry gs_sphere		
			#pragma fragment fs_sphere2

			ENDCG
		}
	}
	Fallback Off
}