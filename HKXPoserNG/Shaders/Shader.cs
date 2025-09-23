using Vortice.Direct3D11;

namespace HKXPoserNG.Shaders;

public abstract class Shader {
    public abstract string HlslPath { get; }

    protected ID3D11InputLayout? inputLayout;
    public ID3D11InputLayout InputLayout => inputLayout!;

    protected ID3D11VertexShader? vertexShader;
    public ID3D11VertexShader VertexShader => vertexShader!;

    protected ID3D11GeometryShader? geometryShader;
    public ID3D11GeometryShader? GeometryShader => geometryShader;

    protected ID3D11PixelShader? pixelShader;
    public ID3D11PixelShader PixelShader => pixelShader!;

    public void Use(ID3D11DeviceContext context) {
        context.IASetInputLayout(inputLayout);
        context.VSSetShader(vertexShader);
        if (geometryShader != null)
            context.GSSetShader(geometryShader);
        context.PSSetShader(pixelShader);
    }
}

