//	Matrices and camera position.
float4x4 matWorld;
float4x4 matView;
float4x4 matProjection;
float3	 EyePosition;

//	Default forward-lighting values.
float3	 DiffuseColor	= float3(1.0, 1.0, 1.0);
float3	 AmbientColor	= float3(0.1, 0.1, 0.1);
float3	 LightDirection	= float3(1.0, 1.0, 1.0);
float3	 LightColor		= float3(0.9, 0.9, 0.9);
float3	 SpecularColor	= float3(1.0, 1.0, 1.0);
float	 SpecularPower	= 32;
bool	 TextureEnabled	= false;

//	Basic texture sampler.
texture2D t_Basic;
sampler s_Basic			= sampler_state {
	texture				= <t_Basic>;
	MinFilter			= Anisotropic;
	MagFilter			= Anisotropic;
	MipFilter			= Linear;
	AddressU			= Wrap;
	AddressV			= Wrap;
};

//	Input & Output Structures.
struct VertexInput {
	float4 Position							: POSITION0;
	float2 TexCoord							: TEXCOORD0;
	float3 Normal							: NORMAL0;
};

struct VertexOutput {
	float4 Position							: POSITION0;
	float2 TexCoord							: TEXCOORD0;
	float3 Normal							: TEXCOORD1;
	float3 ViewDirection					: TEXCOORD2;
};

//	Vertex Shader Function.
VertexOutput VS_Main(in float4 inPosition	: POSITION0,
					 in float2 inTexCoord	: TEXCOORD0,
					 in float3 inNormal		: NORMAL0	)
{
	VertexOutput output				= (VertexOutput)0;
	float4x4 matViewProjection		= mul(matView, matProjection);
	float4 vWorldPosition			= mul(inPosition, matWorld);
	output.Position					= mul(vWorldPosition, matViewProjection);
	output.TexCoord					= inTexCoord;
	output.Normal					= mul(inNormal, matWorld);
	output.ViewDirection			= vWorldPosition - EyePosition;

	return output;
}

float4 PS_Main(in VertexOutput input) : COLOR0
{
	float3 vColor					= DiffuseColor;
	if (TextureEnabled) {
		   vColor				   *= tex2D(s_Basic, input.TexCoord);
	}

	float3 vLighting				= AmbientColor;
	float3 vLightDirection			= normalize(LightDirection);
	float3 vNormal					= normalize(input.Normal);
	vLighting					   += saturate(dot(vLightDirection, vNormal)) * LightColor;
	float3 vReflection				= reflect(vLightDirection, vNormal);
	float3 vViewDirection			= normalize(input.ViewDirection);
	float3 vDot						= dot(vReflection, vViewDirection);
	vLighting					   += pow(saturate(vDot), SpecularPower) * SpecularPower;
	float3 output					= saturate(vLighting) * vColor;
	return float4(output.xyz, 1);
}

technique ForwardLighting
{
	pass Pass1
	{
		VertexShader	= compile vs_1_1 VS_Main();
		PixelShader		= compile ps_2_0 PS_Main();
	}
}			  