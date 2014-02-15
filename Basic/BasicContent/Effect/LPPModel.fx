float4x4 matWorld;
float4x4 matView;
float4x4 matProjection;

float3 AmbientColor = float3(0.15, 0.15, 0.15);
float3 DiffuseColor;
bool TextureEnabled = true;

texture2D t_Basic;
texture2D t_Light;

sampler2D s_Basic = sampler_state {
	texture		= <t_Basic>;
	MinFilter	= Anisotropic;
	MagFilter	= Anisotropic;
	MipFilter	= Linear;
	AddressU	= Wrap;
	AddressV	= Wrap;
};

sampler2D s_Light = sampler_state {
	texture		= <t_Light>;
	MinFilter	= POINT;
	MagFilter	= POINT;
	MipFilter	= POINT;
};

#include "Common.vsi"

struct VertexInput {
	float4 Position			: POSITION0;
	float2 TexCoord			: TEXCOORD0;
};

struct VertexOutput {
	float4 Position			: POSITION0;
	float2 TexCoord			: TEXCOORD0;
	float4 PositionCopy		: TEXCOORD1;
};

VertexOutput VS_Model(VertexInput input)
{
	VertexOutput output			= (VertexOutput)0;
	float4x4 matWorldViewProj	= mul(matWorld, mul(matView, matProjection));
	output.Position				= mul(input.Position, matWorldViewProj);
	output.PositionCopy			= output.Position;
	output.TexCoord				= input.TexCoord;
	return output;
}

float4 PS_Model(VertexOutput input) : COLOR0
{
	float3 basicTexture			= tex2D(s_Basic, input.TexCoord);

	if (!TextureEnabled) { 
		basicTexture			= float4(1.0, 1.0, 1.0, 1.0);
	}

	float2 texCoord				= PostProjToScreen(input.PositionCopy) + HalfPixel();
	float3 light				= tex2D(s_Light, texCoord);
	light					   += AmbientColor;
	return float4(basicTexture * DiffuseColor * light, 1);
}

technique Model
{
	pass Pass1
	{
		VertexShader	= compile vs_1_1 VS_Model();
		PixelShader		= compile ps_2_0 PS_Model();
	}
}