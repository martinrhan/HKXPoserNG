struct VS_IN
{
    float3 position : POSITION;
    float3 color : COLOR;
};

struct PS_IN
{
    float4 position : SV_Position;
    float3 color : COLOR;
};

struct PS_OUT
{
    float4 color : SV_Target;
    //float depth : SV_Depth;
};

PS_IN VS(VS_IN vs_in)
{
    PS_IN output;
    output.position = float4(vs_in.position, 1);
    output.color = vs_in.color;
    return output;
}

PS_OUT PS(PS_IN ps_in) : SV_Target
{
    PS_OUT output;
    output.color = float4(0, 0, 0, 1);
    output.color = float4(ps_in.color, 1);
    return output;
}
