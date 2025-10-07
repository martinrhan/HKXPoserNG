cbuffer cb : register(b0) {
    float4x4 wvp;
    int2 rts;
}

struct VS_IN {
    float3 position : POSITION;
};

struct VS_OUT {
    float4 position : SV_Position;
};

VS_OUT VS(VS_IN input) {
    VS_OUT output;
    float4 p = float4(input.position, 1);
    p = mul(wvp, p);
    float aspectRatio = (float) rts.y / (float) rts.x;
    p.x *= aspectRatio;
    output.position = p;
    return output;
}

float4 PS(VS_OUT input) : SV_Target {
    return float4(0, 0, 0, 1);
}
