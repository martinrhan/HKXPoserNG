cbuffer cb : register(b0) {
    float4x4 wvp;
    int2 rts;
}

// update by submesh, for VS
cbuffer cb_submesh : register(b1) {
    float4x4 palette[40];
}

// update by mesh, for PS
cbuffer cb_mesh : register(b2) {
    unsigned int SLSF1;
    unsigned int SLSF2;
    unsigned int unknown2;
    unsigned int unknown3;
}

struct VS_IN {
    float3 position : POSITION;
    float3 normal : NORMAL;
    float4 weights : BLENDWEIGHT;
    uint4 indices : BLENDINDICES;
};

struct PS_IN {
    float4 position : SV_Position;
    float color : COLOR;
};

PS_IN VS(VS_IN input) {
    PS_IN output;
    float4x4 mat =
        palette[input.indices.x] * (float) input.weights.x +
		palette[input.indices.y] * (float) input.weights.y +
		palette[input.indices.z] * (float) input.weights.z +
		palette[input.indices.w] * (float) input.weights.w;
    
    float4 p = float4(input.position, 1);
    p = mul(mat, p);
    p = mul(wvp, p);
    
    float aspectRatio = (float) rts.y / (float) rts.x;
    p.x *= aspectRatio;
    
    float4 n_4 = float4(input.normal, 0);
    n_4 = mul(mat, n_4);
    n_4 = mul(wvp, n_4);
    float3 n = n_4.xyz;
    
    output.position = p;
    float c = (1 - n.z) * .5;
    output.color = c;
    return output;
}

float4 PS(PS_IN input) : SV_Target {
    float c = input.color;
    return float4(c, c, c, 1);
}
