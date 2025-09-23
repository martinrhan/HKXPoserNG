using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Diagnostics;
using System.Drawing;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace AvaloniaDX;

public class D3D11Control : Control {
    public D3D11Control(PixelSize renderTargetSize) {
        swapChain =
        GlobalObjects.DXGIFactory.CreateSwapChainForComposition(
            GlobalObjects.D3D11Device,
            new SwapChainDescription1 {
                BufferCount = 2,
                Width = (uint)renderTargetSize.Width,
                Height = (uint)renderTargetSize.Height,
                Format = Format.R8G8B8A8_UNorm,
                AlphaMode = AlphaMode.Ignore,
                BufferUsage = Usage.RenderTargetOutput | Usage.Shared,
                SwapEffect = SwapEffect.FlipSequential,
                Scaling = Scaling.Stretch,
                Flags = SwapChainFlags.None
            }
        ).QueryInterface<IDXGISwapChain3>();
        uint backBufferIndex = swapChain.CurrentBackBufferIndex;
        ID3D11Texture2D backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(backBufferIndex);
        var renderTargetView = GlobalObjects.D3D11Device.CreateRenderTargetView1(backBuffer); 
        renderTargetView.Device.ImmediateContext.ClearRenderTargetView(renderTargetView, new Color4(0xff0000ff));
        swapChain.Present(1, PresentFlags.None);

        uint frontBufferIndex = 1 - swapChain.CurrentBackBufferIndex;
        ID3D11Texture2D frontBuffer = swapChain.GetBuffer<ID3D11Texture2D>(frontBufferIndex);
        IDXGIResource1 dxgiResource1 = frontBuffer.QueryInterface<IDXGIResource1>();
        IntPtr ptr = dxgiResource1.CreateSharedHandle(null, Vortice.DXGI.SharedResourceFlags.Read, null);
        PlatformHandle platformHandle = new(ptr, KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle);

        Compositor compositor = ElementComposition.GetElementVisual(this)!.Compositor;
        var task = compositor.TryGetCompositionGpuInterop();
        ICompositionGpuInterop? gpuInterop = task.Result;
        if (gpuInterop is null) return;
        importedImage = gpuInterop.ImportImage(
            platformHandle,
            new() {
                Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                Width = renderTargetSize.Width,
                Height = renderTargetSize.Height,
                TopLeftOrigin = true
            }
            );

        CompositionSurfaceVisual visual = compositor.CreateSurfaceVisual();
        visual.Size = new(Bounds.Width, Bounds.Height);
        compositionDrawingSurface = compositor.CreateDrawingSurface();
        visual.Surface = compositionDrawingSurface;
        ElementComposition.SetElementChildVisual(this, visual);

    }

    private ICompositionImportedGpuImage? importedImage;
    private CompositionDrawingSurface? compositionDrawingSurface;

    private IDXGISwapChain3 swapChain;
    
}

