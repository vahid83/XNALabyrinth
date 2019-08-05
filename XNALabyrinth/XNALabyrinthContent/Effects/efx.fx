//=============================================================================
// 	[GLOBALS]
//=============================================================================

float4x4 mWorldViewProj : WORLDVIEWPROJECTION;

//=============================================================================
// 	[TEXTURES]
//=============================================================================
Texture xTexture;
sampler TextureSampler = sampler_state { 
	texture = <xTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = CLAMP; 
	AddressV = CLAMP;
};

//=============================================================================
// 	[FUNCTIONS]
//=============================================================================

float4 VertexShaderMain(float4 input : POSITION0 ) : POSITION0
{
    float4 output = mul(input, mWorldViewProj);    
    
    return output;
}

float4 PixelShaderMain(float4 input : POSITION0 ) : COLOR0
{
	return float4(1, 0, 0, 0);
}

//=============================================================================
//	[TECHNIQUES]
//=============================================================================

technique BaseTech
{
    pass BasePass
    {
        VertexShader = compile vs_3_0 VertexShaderMain();
        PixelShader  = compile ps_3_0 PixelShaderMain();
    }
}
