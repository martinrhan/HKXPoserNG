using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace AvaloniaDX;

public static class GlobalObjects{
    static GlobalObjects() {
    }
    public static IDXGIFactory7 DXGIFactory = DXGI.CreateDXGIFactory1<IDXGIFactory7>();
    public static ID3D11Device3 D3D11Device = D3D11.D3D11CreateDevice(DriverType.Hardware, DeviceCreationFlags.BgraSupport, Vortice.Direct3D.FeatureLevel.Level_11_1).QueryInterface<ID3D11Device3>();
    public static IDXGIDevice DXGIDevice = D3D11Device.QueryInterface<IDXGIDevice>();
    public static ID2D1Factory1 D2D1Factory = D2D1.D2D1CreateFactory<ID2D1Factory1>();
    public static ID2D1Device D2D1Device = D2D1Factory.CreateDevice(DXGIDevice);
}
