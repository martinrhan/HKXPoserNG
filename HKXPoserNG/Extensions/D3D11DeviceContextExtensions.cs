using Vortice;
using Vortice.Direct3D11;

namespace HKXPoserNG.Extensions;

public static class D3D11DeviceContextExtensions {
    public static void WriteBuffer<T>(this ID3D11DeviceContext context, ID3D11Buffer buffer, T data) where T : unmanaged {
        MappedSubresource map = context.Map(buffer, MapMode.WriteDiscard);
        UnsafeUtilities.Write(map.DataPointer, ref data);
        context.Unmap(buffer);
    }

    public static void WriteBuffer<T>(this ID3D11DeviceContext context, ID3D11Buffer buffer, T[] data) where T : unmanaged {
        MappedSubresource map = context.Map(buffer, MapMode.WriteDiscard);
        UnsafeUtilities.Write(map.DataPointer, data);
        context.Unmap(buffer);
    }

}