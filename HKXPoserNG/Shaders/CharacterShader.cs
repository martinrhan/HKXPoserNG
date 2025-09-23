using SingletonSourceGenerator.Attributes;
using System.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HKXPoserNG.Shaders;

[Singleton]
public partial class CharacterShader : Shader {
    public override string HlslPath => Path.Combine(PathConstants.AppDirectory, "Shaders", "shader_character.hlsl");

    public CharacterShader() {
        var bytecode_vs = Compiler.CompileFromFile(HlslPath, "VS", "vs_5_0");
        InputElementDescription[] elements = [
                new ("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new ("TEXCOORD", 0, Format.R32G32_Float, 0, 1),
                new ("BLENDWEIGHT", 0, Format.R32G32B32A32_Float, 0, 2),
                new ("BLENDINDICES", 0, Format.R8G8B8A8_UInt, 0, 3)
            ];
        inputLayout = DXObjects.D3D11Device.CreateInputLayout(elements, bytecode_vs.Span);
        vertexShader = DXObjects.D3D11Device.CreateVertexShader(bytecode_vs.Span);

        var bytecode_ps = Compiler.CompileFromFile(HlslPath, "PS", "ps_5_0");
        pixelShader = DXObjects.D3D11Device.CreatePixelShader(bytecode_ps.Span);

    }
}

