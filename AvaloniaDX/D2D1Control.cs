using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Diagnostics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace AvaloniaDX;

public class D2D1Control : Control {
    public D2D1Control(PixelSize renderTargetSize) {
        RenderTargetSize = renderTargetSize;
    }
    private ID3D11Texture2D? texture;
    private IDXGIKeyedMutex? mutex;
    private ID2D1RenderTarget? renderTarget;
    private ICompositionImportedGpuImage? importedImage;
    private CompositionDrawingSurface? compositionDrawingSurface;

    public ID2D1RenderTarget RenderTarget => renderTarget!;

    public PixelSize RenderTargetSize { get; }

    protected override void OnLoaded(RoutedEventArgs e) {
        var size = RenderTargetSize;

        texture = GlobalObjects.D3D11Device.CreateTexture2D1(new() {
            Format = Format.R8G8B8A8_UNorm,
            Width = (uint)size.Width,
            Height = (uint)size.Height,
            ArraySize = 1,
            MipLevels = 1,
            SampleDescription = SampleDescription.Default,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            Usage = ResourceUsage.Default,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = ResourceOptionFlags.SharedNTHandle | ResourceOptionFlags.SharedKeyedMutex,
            TextureLayout = TextureLayout.Undefined
        });

        mutex = texture.QueryInterface<IDXGIKeyedMutex>();
        //mutex.AcquireSync(0, int.MaxValue);
        //ID3D11RenderTargetView1 renderTargetView = GlobalObjects.D3D11Device.CreateRenderTargetView1(texture);
        //ID3D11DeviceContext3 context = GlobalObjects.D3D11Device.ImmediateContext3;
        //context.ClearRenderTargetView(renderTargetView, new(0xff0000ff));
        //context.Flush();
        //mutex.ReleaseSync(1);

        renderTarget = GlobalObjects.D2D1Factory.CreateDxgiSurfaceRenderTarget(
            texture.QueryInterface<IDXGISurface>(),
            new() {
                Type = RenderTargetType.Hardware,
                PixelFormat = new(Format.Unknown, Vortice.DCommon.AlphaMode.Ignore),
                Usage = RenderTargetUsage.None,
                MinLevel = Vortice.Direct2D1.FeatureLevel.Default,
                DpiX = 0,
                DpiY = 0
            }
            );

        IDXGIResource1 dxgiResource1 = texture.QueryInterface<IDXGIResource1>();
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
                Width = size.Width,
                Height = size.Height,
                TopLeftOrigin = true
            }
            );

        CompositionSurfaceVisual visual = compositor.CreateSurfaceVisual();
        visual.Size = new(Bounds.Width, Bounds.Height);
        compositionDrawingSurface = compositor.CreateDrawingSurface();
        visual.Surface = compositionDrawingSurface;
        ElementComposition.SetElementChildVisual(this, visual);

        base.OnLoaded(e);
    }

    public void BeginDraw() {
        mutex!.AcquireSync(0, int.MaxValue);
        renderTarget!.BeginDraw();
    }
    public void EndDraw() {
        renderTarget!.EndDraw();
        mutex!.ReleaseSync(1);
        drawTask = compositionDrawingSurface!.UpdateWithKeyedMutexAsync(importedImage!, 1, 0);
    }
    private Task? drawTask;
    public bool IsLastDrawCompleted => drawTask?.IsCompleted ?? true;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        compositionDrawingSurface?.Dispose();
        renderTarget?.Dispose();
        mutex?.Dispose();
        texture?.Dispose();
    }
}

