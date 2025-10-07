cbuffer cb : register(b0) {
    float4x4 wvp;
    int2 rts;
}

cbuffer cb_bone : register(b1) {
    int selectedBone;
}

struct VS_IN {
    float3 position : POSITION;
};

struct VS_OUT {
    float4 position : SV_Position;
    float4 color : COLOR;
};

struct GS_OUT {
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

struct PS_OUT {
    float4 color : SV_Target;
};

VS_OUT VS(VS_IN input, uint vid : SV_VertexID) {
    VS_OUT output;
    
    float4 p = float4(input.position, 1);
    p = mul(wvp, p);
    float aspectRatio = (float) rts.y / (float) rts.x;
    p.x *= aspectRatio;
    output.position = p;
    
    if (vid == selectedBone) {
        output.color = float4(0, 0, 1, 1);
    }
    else {
        output.color = float4(1, 0, 0, 1);
    }
    
    return output;
}

[maxvertexcount(4)]
void GS(point VS_OUT vin[1], inout TriangleStream<GS_OUT> outStream) {
    float aspectRatio = (float) rts.y / (float) rts.x;

    float4 center = vin[0].position;
    center /= center.w;
    GS_OUT output00, output01, output10, output11;
    output00.position = center + float4(-.01 * aspectRatio, -.01, 0, 0);
    output01.position = center + float4(-.01 * aspectRatio, .01, 0, 0);
    output10.position = center + float4(.01 * aspectRatio, -.01, 0, 0);
    output11.position = center + float4(.01 * aspectRatio, .01, 0, 0);
    output00.color = output01.color = output10.color = output11.color = vin[0].color;
    outStream.Append(output00);
    outStream.Append(output01);
    outStream.Append(output10);
    outStream.Append(output11);
}

PS_OUT PS(GS_OUT input) : SV_Target {
    PS_OUT output;
    output.color = input.color;
    return output;
}
