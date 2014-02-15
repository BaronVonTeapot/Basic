float4x4 matWorld;
float4x4 matView;
float4x4 matProjection;

struct VertexInput {
	float4 Position				: POSITION0;
	float3 Normal				: NORMAL0;
};

struct VertexOutput {
	float4 Position				: POSITION0;
	float2 Depth				: TEXCOORD0;
	float3 Normal				: TEXCOORD1;
};

struct PixelOutput {
	float4 Normal				: COLOR0;
	float4 Depth				: COLOR1;
};

VertexOutput VS_DepthNormal(VertexInput input)
{
	VertexOutput output			= (VertexOutput)0;
	float4x4 matViewProjection	= mul(matView, matProjection);
	float4x4 matWorldViewProj	= mul(matWorld, matViewProjection);

	output.Position				= mul(input.Position, matWorldViewProj);
	output.Normal				= mul(input.Normal, matWorld);
	output.Depth.xy				= output.Position.zw;

	return output;
}

PixelOutput PS_DepthNormal(VertexOutput input)
{
	PixelOutput output			= (PixelOutput)0;
	output.Depth				= input.Depth.x / input.Depth.y;
	output.Depth.a				= 1;
	output.Normal.xyz			= (normalize(input.Normal).xyz / 2) + 0.5;
	output.Normal.a				= 1;
	
	return output;
}

technique DepthNormal
{
	pass Pass1
	{
		VertexShader	= compile vs_2_0 VS_DepthNormal();
		PixelShader		= compile ps_2_0 PS_DepthNormal();
	}
}