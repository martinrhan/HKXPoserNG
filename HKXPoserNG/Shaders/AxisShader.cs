using SingletonSourceGenerator.Attributes;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HKXPoserNG.Shaders;

[Singleton]
public partial class AxisShader : Shader {
    public override string HlslPath => Path.Combine(PathConstants.AppDirectory, "Shaders", "shader_axis.hlsl");

    public AxisShader() {
        var bytecode_vs = Compiler.CompileFromFile(HlslPath, "VS", "vs_5_0");
        InputElementDescription[] elements = [
                new ("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new ("COLOR", 0, Format.R32G32B32_Float, 0, 1)
            ];
        inputLayout = DXObjects.D3D11Device.CreateInputLayout(elements, bytecode_vs.Span);
        vertexShader = DXObjects.D3D11Device.CreateVertexShader(bytecode_vs.Span);

        var bytecode_ps = Compiler.CompileFromFile(HlslPath, "PS", "ps_5_0");
        pixelShader = DXObjects.D3D11Device.CreatePixelShader(bytecode_ps.Span);

    }
}

