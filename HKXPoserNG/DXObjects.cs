using System.Numerics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HKXPoserNG;

public static class DXObjects {
    public static IDXGIFactory7 DXGIFactory = DXGI.CreateDXGIFactory1<IDXGIFactory7>();

    public static ID3D11Device3 D3D11Device = D3D11
        .D3D11CreateDevice(DriverType.Hardware, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_1)
        .QueryInterface<ID3D11Device3>();

    public static IDXGIDevice DXGIDevice = D3D11Device.QueryInterface<IDXGIDevice>();

    public static ID3D11Buffer VertexBuffer_FullScreenRectangle = D3D11Device.CreateBuffer(
        new Vector2[] {
            new(-1, -1),
            new(-1, 1),
            new(1, -1),
            new(1f, 1f)
        },
        BindFlags.VertexBuffer,
        ResourceUsage.Immutable
    );

    public static ID3D11Buffer IndexBuffer_FullScreenRectangle = D3D11Device.CreateBuffer(
        new ushort[] {
            0, 1, 2,
            1, 3, 2
        },
        BindFlags.IndexBuffer,
        ResourceUsage.Immutable
    );
}

public class Shader {
    const string Path = "";
}