using NiflySharp.Structs;
using Pfim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vortice;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HKXPoserNG.ViewModels;

public class Texture {
    private static Dictionary<string, Texture> loadedTextures = new();

    public static Texture GetOrCreate(string fileName) {
        if (loadedTextures.TryGetValue(fileName, out var existingTexture)) {
            return existingTexture;
        }
        var newTexture = new Texture(fileName);
        loadedTextures[fileName] = newTexture;
        return newTexture;
    }

    private Texture(string fileName) {
        string path = Path.Combine(PathConstants.DataDirectory, "textures", Path.GetFileName(fileName));
        Dds dds;
        using (var stream = File.OpenRead(path)) {
            dds = Dds.Create(stream, new());
        }
        SubresourceData ssd = new(Marshal.UnsafeAddrOfPinnedArrayElement(dds.Data, 0), (uint)dds.Width * 4);
        D3DTexture2D = DXObjects.D3D11Device.CreateTexture2D(
            new Texture2DDescription {
                Width = (uint)dds.Width,
                Height = (uint)dds.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                SampleDescription = new SampleDescription(1, 0),
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            },
            ssd);
        D3DShaderResourceView = DXObjects.D3D11Device.CreateShaderResourceView(D3DTexture2D);
    }

    public ID3D11Texture2D D3DTexture2D { get; }
    public ID3D11ShaderResourceView D3DShaderResourceView { get; }
}

