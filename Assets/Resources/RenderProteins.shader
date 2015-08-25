Shader "Custom/RenderProteins" 
{	
	CGINCLUDE
		
	#include "UnityCG.cginc"
	#include "Helper.cginc"		
	
	#define MAX_SUBINSTANCE_SIZE 4096

	uniform float _Scale;	
	uniform int _EnableLod;
	uniform float3 _CameraForward;	
	uniform int _FirstLevelBeingRange;

	uniform StructuredBuffer<float4> _LodLevelsInfos;	
	uniform	StructuredBuffer<float4> _ProteinInstanceInfo;
	uniform	StructuredBuffer<float4> _ProteinInstancePositions;
	uniform	StructuredBuffer<float4> _ProteinInstanceRotations;
						
	uniform StructuredBuffer<float4> _ProteinColors;	
	uniform StructuredBuffer<float4> _ProteinAtomPositions;	
	uniform StructuredBuffer<float4> _ProteinClusterPositions;	
	uniform StructuredBuffer<int4> _ProteinSphereBatchInfos;	
	
	//--------------------------------------------------------------------------------------

	struct hsConst
	{
		float tessFactor[2] : SV_TessFactor;
	};

	struct ds_input
	{
		int id : INT0;
		int type : INT1;
		int state : INT2;	
		int sphereStart : INT3;	
		int sphereCount : INT4;	
		int lodLevel : INT5;	
		int decimationFactor : INT6;		
		float radiusScale : FLOAT0;
				
		float3 pos : FLOAT30;
		float3 color : FLOAT31;				
		float4 rot : FLOAT40;	
	};

	struct gs_input
	{
		int id : INT0;		
		int type : INT1;
		int state : INT2;								
		float radius : FLOAT0;	
		
		float3 pos : FLOAT30;	
		float3 color : FLOAT31;		
	};

	struct fs_input
	{
		nointerpolation int id : INT0;				
		nointerpolation float radius : FLOAT0;	
		nointerpolation float3 color : FLOAT30;		
				
		float2 uv: TEXCOORD0;	
		centroid float4 pos : SV_Position;	
	};
	
	//--------------------------------------------------------------------------------------
	
	void vs_protein(uint id : SV_VertexID, out ds_input output)
	{		
		int4 sphereBatchInfo = _ProteinSphereBatchInfos[id];	
		
		float4 infos = _ProteinInstanceInfo[sphereBatchInfo.x];		
		float4 sphere = _ProteinInstancePositions[sphereBatchInfo.x] * _Scale;	
		
		output.id = sphereBatchInfo.x;
		output.type = infos.x;		
		output.state = infos.y;		
		output.rot = _ProteinInstanceRotations[output.id];	
		output.pos = sphere.xyz;
		output.sphereCount = sphereBatchInfo.z;	
		output.sphereStart = sphereBatchInfo.w;
		
		// Set LOD values	
		float beginRange = (sphereBatchInfo.y == 0) ? _FirstLevelBeingRange : _LodLevelsInfos[sphereBatchInfo.y -1].x;
		float endRange = max(_LodLevelsInfos[sphereBatchInfo.y].x, beginRange);
		float cameraDistance = min(max(dot(output.pos - _WorldSpaceCameraPos, _CameraForward), beginRange), endRange);			
		float radiusLerp = saturate((cameraDistance - beginRange) / (endRange - beginRange)); 	
		float radiusMin =  max(_LodLevelsInfos[sphereBatchInfo.y].y, 1);
		float radiusMax = max(_LodLevelsInfos[sphereBatchInfo.y].z, radiusMin);
		
		output.lodLevel = (_EnableLod) ? sphereBatchInfo.y : 0;		
		output.radiusScale = ((_EnableLod) ? lerp(radiusMin, radiusMax, radiusLerp) : 1) * _Scale;				
	}	

	//--------------------------------------------------------------------------------------

	void HSConst(InputPatch<ds_input, 1> input, uint patchID : SV_PrimitiveID, out hsConst output)
	{
		output.tessFactor[0] = output.tessFactor[1] = ( input[0].sphereCount <= 0 ) ? 0 : ceil(sqrt(input[0].sphereCount));									
		return;
	}

	[domain("isoline")]
	[partitioning("integer")]
	[outputtopology("point")]
	[outputcontrolpoints(1)]				
	[patchconstantfunc("HSConst")]
	ds_input hs_protein (InputPatch<ds_input, 1> input, uint ID : SV_OutputControlPointID)
	{
		return input[0];
	} 

	//--------------------------------------------------------------------------------------
			
	[domain("isoline")]
	void ds_protein(hsConst input, const OutputPatch<ds_input, 1> op, float2 uv : SV_DomainLocation, out gs_input output)
	{
		int x = round(uv.y * input.tessFactor[0]);
		int y = round(uv.x * input.tessFactor[0]);		
		int sphereIndex = x + y * input.tessFactor[0];							
		float4 spherePosition = (op[0].lodLevel == 0) ? _ProteinAtomPositions[op[0].sphereStart + sphereIndex] : _ProteinClusterPositions[op[0].sphereStart + sphereIndex];				
	
		output.id = op[0].id;
		output.type = op[0].type;
		output.state = op[0].state;
		output.color = op[0].color;
		output.pos = op[0].pos + QuaternionTransform(op[0].rot, spherePosition.xyz) * _Scale;	
		output.radius = (y >= input.tessFactor[0] || sphereIndex >= op[0].sphereCount) ? 0 : max(spherePosition.w * _Scale, 1 * op[0].radiusScale); // Discard unwanted spheres	
	}

	//--------------------------------------------------------------------------------------
							
	[maxvertexcount(3)]
	void gs_protein(point gs_input input[1], inout TriangleStream<fs_input> triangleStream)
	{
		// Discard unwanted atoms
		if( input[0].radius <= 0 ) return;

		float4 viewPos = mul(UNITY_MATRIX_MV, float4(input[0].pos, 1));
		viewPos -= normalize( viewPos ) * input[0].radius;
		float4 projPos = mul(UNITY_MATRIX_P, float4(viewPos.xyz, 1));
		float4 offset = mul(UNITY_MATRIX_P, float4(input[0].radius, input[0].radius, 0, 0));

		fs_input output;	
		output.id = input[0].id;		
		output.color = input[0].color;			
		output.radius = input[0].radius;

		//*****//
		
		float triBase = 3.464;
		float triHeigth = 3;
		float triBaseHalf = triBase * 0.5;
		float2 triOffset = float2(triBaseHalf, 1.0);

		output.uv = float2(0, 0) - triOffset;
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(triBaseHalf, triHeigth) - triOffset;
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);	
								
		output.uv = float2(triBase,0) - triOffset;
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);
	}

	//--------------------------------------------------------------------------------------
	
	void fs_protein(fs_input input, out int id : SV_TARGET0, out float depth : sv_depthgreaterequal) 
	{	
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;
				
		// Find normal
		float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));		

		// Find depth
		float eyeDepth = LinearEyeDepth(input.pos.z) + input.radius * (1-normal.z);
		depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;	
						
		// Output id			
		id = input.id; 		
	}

	ENDCG
	
	//--------------------------------------------------------------------------------------

	SubShader 
	{	
		Pass 
	    {
			ZTest Lequal
			ZWrite On

	    	CGPROGRAM			
	    		
			#include "UnityCG.cginc"
			
			#pragma only_renderers d3d11
			#pragma target 5.0				
			
			#pragma vertex vs_protein
			#pragma hull hs_protein
			#pragma domain ds_protein				
			#pragma geometry gs_protein			
			#pragma fragment fs_protein
						
			ENDCG
		}	
	}
	Fallback Off
}	