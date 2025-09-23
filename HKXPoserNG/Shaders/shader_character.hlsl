cbuffer cb : register(b0)
{
    float4x4 wvp;
    int2 rts;
}

// update by submesh, for VS
cbuffer cb_submesh : register(b1)
{
    float4x4 palette[40];
}

// update by mesh, for PS
cbuffer cb_mesh : register(b2)
{
    unsigned int SLSF1;
    unsigned int SLSF2;
    unsigned int unknown2;
    unsigned int unknown3;
}


struct VS_IN
{
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
    float4 weights : BLENDWEIGHT;
    uint4 indices : BLENDINDICES;
};

struct PS_IN
{
    float4 position : SV_Position;
    float2 texcoord : TEXCOORD;
};

Texture2D albedoMap;
SamplerState albedoSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

PS_IN VS(VS_IN vs_in)
{
    PS_IN output;
    float4 p = float4(vs_in.position, 1);
    float4x4 mat =
        palette[vs_in.indices.x] * (float) vs_in.weights.x +
		palette[vs_in.indices.y] * (float) vs_in.weights.y +
		palette[vs_in.indices.z] * (float) vs_in.weights.z +
		palette[vs_in.indices.w] * (float) vs_in.weights.w;
    p = mul(mat, p);
    p = mul(wvp, p);
    output.position = p;
    output.texcoord = vs_in.texcoord;
    return output;
}


float4 PS(PS_IN ps_in) : SV_Target
{
    bool model_space_normals = SLSF1 & (1 << 12);

    float4 albedo = albedoMap.Sample(albedoSampler, ps_in.texcoord);
    clip(albedo.a - 0.25); // alpha test

    if (model_space_normals)
        albedo += albedo;

    return albedo;
}
