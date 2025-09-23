using SingletonSourceGenerator.Attributes;
using System.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HKXPoserNG.Shaders;

[Singleton]
public partial class SkeletonPointsShader : Shader {
    public override string HlslPath => Path.Combine(PathConstants.AppDirectory, "Shaders", "shader_skeleton_points.hlsl");

    public SkeletonPointsShader() {
        var bytecode_vs = Compiler.CompileFromFile(HlslPath, "VS", "vs_5_0");
        InputElementDescription[] elements = [
                new ("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            ];
        inputLayout = DXObjects.D3D11Device.CreateInputLayout(elements, bytecode_vs.Span);
        vertexShader = DXObjects.D3D11Device.CreateVertexShader(bytecode_vs.Span);

        var bytecode_gs = Compiler.CompileFromFile(HlslPath, "GS", "gs_5_0");
        geometryShader = DXObjects.D3D11Device.CreateGeometryShader(bytecode_gs.Span);

        var bytecode_ps = Compiler.CompileFromFile(HlslPath, "PS", "ps_5_0");
        pixelShader = DXObjects.D3D11Device.CreatePixelShader(bytecode_ps.Span);
    }
}
