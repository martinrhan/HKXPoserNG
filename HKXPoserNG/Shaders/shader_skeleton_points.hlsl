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

struct VS_GS_OUT {
    float4 position : SV_Position;
    float4 color : COLOR;
    int vid : TEXCOORD;
};

struct PS_OUT {
    float4 color : SV_Target0;
    int vid : SV_Target1;
};

VS_GS_OUT VS(VS_IN input, uint vid : SV_VertexID) {
    VS_GS_OUT output;
    
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
    
    output.vid = vid;
    
    return output;
}

[maxvertexcount(4)]
void GS(point VS_GS_OUT vin[1], inout TriangleStream<VS_GS_OUT> outStream) {
    float aspectRatio = (float) rts.y / (float) rts.x;

    float4 center = vin[0].position;
    center /= center.w;
    VS_GS_OUT output00, output01, output10, output11;
    output00.position = center + float4(-.01 * aspectRatio, -.01, 0, 0);
    output01.position = center + float4(-.01 * aspectRatio, .01, 0, 0);
    output10.position = center + float4(.01 * aspectRatio, -.01, 0, 0);
    output11.position = center + float4(.01 * aspectRatio, .01, 0, 0);
    output00.color = output01.color = output10.color = output11.color = vin[0].color;
    output00.vid = output01.vid = output10.vid = output11.vid = vin[0].vid;
    outStream.Append(output00);
    outStream.Append(output01);
    outStream.Append(output10);
    outStream.Append(output11);
}

PS_OUT PS(VS_GS_OUT input) {
    PS_OUT output;
    output.color = input.color;
    output.vid = input.vid;
    return output;
}
