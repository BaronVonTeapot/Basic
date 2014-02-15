float4x4 matWorldViewProjection;
float4x4 matInverseViewProjection;
float3	 LightColor;
float3	 LightPosition;
float	 Attenuation;

texture2D t_Depth;
texture2D t_Normal;

sampler2D s_Depth = sampler_state {
	texture		= <t_Depth>;
	MinFilter	= point;
	MagFilter	= point;
	MipFilter	= point;
};

sampler2D s_Normal = sampler_state {
	texture		= <t_Normal>;
	MinFilter	= point;
	MagFilter	= point;
	MipFilter	= point;
};

#include "Common.vsi"

struct VertexInput {
	float4 Position			: POSITION0;
};

struct VertexOutput {
	float4 Position			: POSITION0;
	float4 LightPosition	: TEXCOORD0;
};

//	Light Pre-Pass Point Light Vertex Shader.
VertexOutput VS_PointLight(VertexInput input)
{
	VertexOutput output		= (VertexOutput)0;
	output.Position			= mul(input.Position, matWorldViewProjection);
	output.LightPosition	= output.Position;

	return output;
}

float4 PS_PointLight(VertexOutput input) : COLOR0
{
	float2 texCoord			= PostProjToScreen(input.LightPosition) + HalfPixel();
	float4 depth			= tex2D(s_Depth, texCoord);
	
	float4 position;
	position.x				= texCoord.x * 2 - 1;
	position.y				= (1 - texCoord.y) * 2 - 1;
	position.z				= depth.r;
	position.w				= 1.0;
	position				= mul(position, matInverseViewProjection);
	position.xyz		   /= position.w;

	float4 normal			= (tex2D(s_Normal, texCoord) - 0.5) * 2;

	float3 lightDirection	= normalize(LightPosition - position);
	float lighting			= clamp(dot(normal, lightDirection), 0, 1);

	float dist				= distance(LightPosition, position);
	float att				= 1 - pow(dist / Attenuation, 6);

	return float4(LightColor * lighting * att, 1);
}

technique PointLight
{
	pass Pass1
	{
		VertexShader	= compile vs_1_1 VS_PointLight();
		PixelShader		= compile ps_2_0 PS_PointLight();
	}
}