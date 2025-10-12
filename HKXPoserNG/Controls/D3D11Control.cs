using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace HKXPoserNG.Controls;

public class D3D11Control : Control, ICustomHitTest {
    public D3D11Control() {
        SinglePixelStagingTexture = DXObjects.D3D11Device.CreateTexture2D1(new() {
            Format = Format.R16_UInt,
            Width = 1,
            Height = 1,
            ArraySize = 1,
            MipLevels = 1,
            SampleDescription = SampleDescription.Default,
            BindFlags = BindFlags.None,
            Usage = ResourceUsage.Staging,
            CPUAccessFlags = CpuAccessFlags.Read,
            TextureLayout = TextureLayout.Undefined,
        });
    }
    public int TextureWidth { get; private set; }
    public int TextureHeight { get; private set; }

    protected ID3D11Texture2D? RenderTargetTexture0 { get; private set; }
    protected ID3D11RenderTargetView? RenderTargetView0 { get; private set; }
    protected ID3D11Texture2D? RenderTargetTexture1 { get; private set; }
    protected ID3D11RenderTargetView? RenderTargetView1 { get; private set; }
    protected ID3D11Texture2D? DepthStencilTexture { get; private set; }
    protected ID3D11DepthStencilView? DepthStencilView { get; private set; }
    protected ID3D11Texture2D? SinglePixelStagingTexture { get; private set; }

    private ICompositionImportedGpuImage? importedImage;
    private CompositionDrawingSurface? compositionDrawingSurface;

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        RenderTargetView0?.Dispose();
        RenderTargetTexture0?.Dispose();
        RenderTargetView1?.Dispose();
        RenderTargetTexture1?.Dispose();
        DepthStencilView?.Dispose();
        DepthStencilTexture?.Dispose();
        var pixelSize = PixelSize.FromSize(e.NewSize, VisualRoot!.RenderScaling);
        TextureWidth = pixelSize.Width;
        TextureHeight = pixelSize.Height;
        RenderTargetTexture0 = DXObjects.D3D11Device.CreateTexture2D1(new() {
            Format = Format.R8G8B8A8_UNorm,
            Width = (uint)TextureWidth,
            Height = (uint)TextureWidth,
            ArraySize = 1,
            MipLevels = 1,
            SampleDescription = SampleDescription.Default,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            Usage = ResourceUsage.Default,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = ResourceOptionFlags.SharedNTHandle | ResourceOptionFlags.SharedKeyedMutex,
        });
        RenderTargetView0 = DXObjects.D3D11Device.CreateRenderTargetView1(RenderTargetTexture0);
        RenderTargetTexture1 = DXObjects.D3D11Device.CreateTexture2D1(new() {
            Format = Format.R16_UInt,
            Width = (uint)TextureWidth,
            Height = (uint)TextureWidth,
            ArraySize = 1,
            MipLevels = 1,
            SampleDescription = SampleDescription.Default,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            Usage = ResourceUsage.Default,
            CPUAccessFlags = CpuAccessFlags.None,
        });
        RenderTargetView1 = DXObjects.D3D11Device.CreateRenderTargetView1(RenderTargetTexture1);
        DepthStencilTexture = DXObjects.D3D11Device.CreateTexture2D1(new() {
            Format = Format.D32_Float_S8X24_UInt,
            Width = (uint)TextureWidth,
            Height = (uint)TextureWidth,
            ArraySize = 1,
            MipLevels = 1,
            SampleDescription = SampleDescription.Default,
            BindFlags = BindFlags.DepthStencil,
            Usage = ResourceUsage.Default,
            CPUAccessFlags = CpuAccessFlags.None,
            TextureLayout = TextureLayout.Undefined
        });
        DepthStencilView = DXObjects.D3D11Device.CreateDepthStencilView(DepthStencilTexture);
        mutex = RenderTargetTexture0.QueryInterface<IDXGIKeyedMutex>();
        Compositor compositor = ElementComposition.GetElementVisual(this)!.Compositor;
        var task = compositor.TryGetCompositionGpuInterop();
        ICompositionGpuInterop? gpuInterop = task.Result;
        if (gpuInterop is null) return;
        IDXGIResource1 dxgiResource1 = RenderTargetTexture0.QueryInterface<IDXGIResource1>();
        IntPtr ptr = dxgiResource1.CreateSharedHandle(null, Vortice.DXGI.SharedResourceFlags.Read, null);
        PlatformHandle platformHandle = new(ptr, KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle);
        importedImage = gpuInterop.ImportImage(
            platformHandle,
            new() {
                Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                Width = TextureWidth,
                Height = TextureHeight,
                TopLeftOrigin = true
            }
        );
        CompositionSurfaceVisual visual = compositor.CreateSurfaceVisual();
        visual.Size = new(TextureWidth, TextureHeight);
        compositionDrawingSurface = compositor.CreateDrawingSurface();
        visual.Surface = compositionDrawingSurface;
        ElementComposition.SetElementChildVisual(this, visual);
    }

    private IDXGIKeyedMutex? mutex;

    protected void BeginDraw() {
        var context = DXObjects.D3D11Device.ImmediateContext;
        mutex!.AcquireSync(0, int.MaxValue);
    }

    protected Task EndDraw() {
        mutex!.ReleaseSync(1);
        Task task = compositionDrawingSurface!.UpdateWithKeyedMutexAsync(importedImage!, 1, 0);
        return task;
    }

    public bool HitTest(Avalonia.Point point) {
        return true;
    }

    public ushort GetPixelFromRenderTargetTexture1(int x, int y) {
        var context = DXObjects.D3D11Device.ImmediateContext;

        Box box = new Box(x, y, 0, x + 1, y + 1, 1);
        context.CopySubresourceRegion(SinglePixelStagingTexture, 0, 0, 0, 0, RenderTargetTexture1, 0, box);
        MappedSubresource map = context.Map(SinglePixelStagingTexture!, 0, MapMode.Read);
        Span<ushort> span = map.AsSpan<ushort>(1);
        ushort result = span[0];
        context.Unmap(SinglePixelStagingTexture!, 0);
        return result; 
    }
}

