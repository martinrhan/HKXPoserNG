using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HKXPoserNG.Controls;
using HKXPoserNG.Extensions;
using HKXPoserNG.Shaders;
using HKXPoserNG.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HKXPoserNG.Views;

public class D3D11View : D3D11Control {
    public D3D11View() {
        this.Focusable = true;
        this.IsHitTestVisible = true;

        Skeleton.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(Skeleton.SelectedBone)) {
                needRedraw = true;
            }
        };
        Animation.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(Animation.CurrentFrame)) {
                needRedraw = true;
            }
        };

        ID3D11DeviceContext context = DXObjects.D3D11Device.ImmediateContext;

        cb0 = DXObjects.D3D11Device.CreateBuffer(
                (uint)Marshal.SizeOf<Matrix4x4>(),
                BindFlags.ConstantBuffer,
                ResourceUsage.Dynamic,
                CpuAccessFlags.Write
            );
        cb_boneMatrices = DXObjects.D3D11Device.CreateBuffer(
                (uint)(40 * Marshal.SizeOf<Matrix4x4>()),
                BindFlags.ConstantBuffer,
                ResourceUsage.Dynamic,
                CpuAccessFlags.Write
            );
        cb_shaderFlags = DXObjects.D3D11Device.CreateBuffer(
                (uint)(4 * Marshal.SizeOf<uint>()),
                BindFlags.ConstantBuffer,
                ResourceUsage.Dynamic,
                CpuAccessFlags.Write
            );

        vb_axis = DXObjects.D3D11Device.CreateBuffer(
            (uint)Marshal.SizeOf<Vector3>() * 10,
            BindFlags.VertexBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write
        );
        vb_axis_color = DXObjects.D3D11Device.CreateBuffer(
            new Vector3[] {
                new(1, 0, 0), new(1, 0, 0),
                new(0, 1, 0), new(0, 1, 0),
                new(0, 0, 1), new(0, 0, 1),
                new(0, 0, 0), new(0, 0, 0),
                new(0, 0, 0), new(0, 0, 0)
            },
            BindFlags.VertexBuffer,
            ResourceUsage.Immutable
        );
        ib_axis = DXObjects.D3D11Device.CreateBuffer(
            new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            BindFlags.IndexBuffer,
            ResourceUsage.Immutable
        );
        cb_bone = DXObjects.D3D11Device.CreateBuffer(
            16,
            BindFlags.ConstantBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write
        );

        rasterizerState = DXObjects.D3D11Device.CreateRasterizerState(
            new() {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None
            }
        );

        //BlendDescription blendDesc = new();
        //blendDesc.RenderTarget[0] = new() {
        //    BlendEnable = true,
        //    SourceBlend = Blend.SourceAlpha,
        //    DestinationBlend = Blend.InverseSourceAlpha,
        //    BlendOperation = BlendOperation.Add,
        //    SourceBlendAlpha = Blend.One,
        //    DestinationBlendAlpha = Blend.Zero,
        //    BlendOperationAlpha = BlendOperation.Add,
        //    RenderTargetWriteMask = ColorWriteEnable.All
        //};
        //ID3D11BlendState blendState = DXObjects.D3D11Device.CreateBlendState(blendDesc);
        //context.OMSetBlendState(blendState);
    }
    private ID3D11Buffer cb0, cb_boneMatrices, cb_shaderFlags, vb_axis, vb_axis_color, ib_axis, cb_bone;
    private ID3D11RasterizerState rasterizerState;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        void DispatcherRecursion(Task lastCompositionTask) {
            if (lastCompositionTask.IsFaulted)
                throw new Exception();
            if (lastCompositionTask.IsCompleted && needRedraw) {
                UpdateConstantBuffer0();
                Task task = Render();
                Dispatcher.UIThread.Post(() => DispatcherRecursion(task), DispatcherPriority.Background);
            } else {
                Dispatcher.UIThread.Post(() => DispatcherRecursion(lastCompositionTask), DispatcherPriority.Background);
            }
        }
        DispatcherRecursion(Task.CompletedTask);
    }

    private Matrix4x4[] array_boneMatrices = new Matrix4x4[40];
    private uint[] array_shaderFlags = new uint[2];

    bool needRedraw = true;
    private int frameCount = 0;
    private Task Render() {
#if DEBUG
        Debug.WriteLine($"Frame {frameCount++}, Focus:{focusPoint}, CamYaw: {cameraYaw}, CamPitch: {cameraPitch}, CamDis: {cameraDistance}, CamPos: {cameraPosition}");
#endif
        ID3D11DeviceContext context = DXObjects.D3D11Device.ImmediateContext;

        void DrawCharacter() {
            CharacterShader.Instance.Use(context);
            context.VSSetConstantBuffer(0, cb0);
            context.PSSetConstantBuffer(0, cb0);
            context.VSSetConstantBuffer(1, cb_boneMatrices);
            context.PSSetConstantBuffer(2, cb_shaderFlags);
            context.RSSetViewport(0, 0, TextureWidth, TextureHeight);
            context.RSSetState(rasterizerState);
            context.OMSetRenderTargets(RenderTargetView!, DepthStencilView);
            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            foreach (var mesh in BodyModel.Instance.Meshes) {
                array_shaderFlags[0] = (uint)mesh.SLSF1;
                array_shaderFlags[1] = (uint)mesh.SLSF2;
                context.WriteBuffer(cb_shaderFlags, array_shaderFlags);
                if (mesh.Texture != null)
                    context.PSSetShaderResource(0, mesh.Texture.D3DShaderResourceView);
                foreach (var partition in mesh.PartitionMeshes) {
                    for (int i = 0; i < partition.BoneMap.Count; i++) {
                        int i_bone_mesh = partition.BoneMap[i];
                        int i_bone_global = mesh.BoneMap[i_bone_mesh];
                        Bone bone = Skeleton.Instance.Bones[i_bone_global];
                        Transform transform = mesh.BoneInverseTransforms[i_bone_mesh] * bone.GlobalTransform;
                        array_boneMatrices[i] = transform.Matrix;
                    }
                    context.WriteBuffer(cb_boneMatrices, array_boneMatrices);
                    context.IASetVertexBuffer(0, mesh.VertexBuffer, (uint)Marshal.SizeOf<Vector3>());
                    context.IASetVertexBuffer(1, mesh.NormalBuffer, (uint)Marshal.SizeOf<Vector3>());
                    context.IASetVertexBuffer(2, mesh.WeightBuffer, (uint)Marshal.SizeOf<Vector4>());
                    context.IASetVertexBuffer(3, mesh.BoneIndexBuffer, 4);
                    context.IASetIndexBuffer(partition.TriangleIndexBuffer, Format.R16_UInt, 0);
                    context.DrawIndexed(partition.TriangleIndexBuffer.Description.ByteWidth / 2, 0, 0);
                }
            }
        }

        void DrawAxis() {
            float aspectRatio = (float)TextureHeight / TextureWidth;

            Vector4 origin = new(0, 0, 0, 1);
            Vector4 origin_clip = Vector4.Transform(origin, cb0_data.wvp);
            Vector3 origin_ndc = new(origin_clip.X / origin_clip.W * aspectRatio, origin_clip.Y / origin_clip.W, origin_clip.Z / origin_clip.W);

            Vector4 unitx = new(10, 0, 0, 1);
            Vector4 unitx_clip = Vector4.Transform(unitx, cb0_data.wvp);
            Vector3 unitx_ndc = new(unitx_clip.X / unitx_clip.W * aspectRatio, unitx_clip.Y / unitx_clip.W, unitx_clip.Z / unitx_clip.W);

            Vector4 unity = new(0, 10, 0, 1);
            Vector4 unity_clip = Vector4.Transform(unity, cb0_data.wvp);
            Vector3 unity_ndc = new(unity_clip.X / unity_clip.W * aspectRatio, unity_clip.Y / unity_clip.W, unity_clip.Z / unity_clip.W);

            Vector4 unitz = new(0, 0, 10, 1);
            Vector4 unitz_clip = Vector4.Transform(unitz, cb0_data.wvp);
            Vector3 unitz_ndc = new(unitz_clip.X / unitz_clip.W * aspectRatio, unitz_clip.Y / unitz_clip.W, unitz_clip.Z / unitz_clip.W);

            Vector4 focus_clip = Vector4.Transform(new Vector4(focusPoint, 1), cb0_data.wvp);
            Vector3 focus_ndc = new(focus_clip.X / focus_clip.W * aspectRatio, focus_clip.Y / focus_clip.W, focus_clip.Z / focus_clip.W);

            Vector3[] axis_vertices = [
                origin_ndc, unitx_ndc,
                origin_ndc, unity_ndc,
                origin_ndc, unitz_ndc,
                focus_ndc with {X = focus_ndc.X - .1f * aspectRatio}, focus_ndc with {X = focus_ndc.X + .1f * aspectRatio},
                focus_ndc with {Y = focus_ndc.Y - .1f}, focus_ndc with {Y = focus_ndc.Y + .1f},
            ];
            context.WriteBuffer(vb_axis, axis_vertices);

            AxisShader.Instance.Use(context);
            context.IASetVertexBuffer(0, vb_axis, (uint)Marshal.SizeOf<Vector3>());
            context.IASetVertexBuffer(1, vb_axis_color, (uint)Marshal.SizeOf<Vector3>());
            context.IASetIndexBuffer(ib_axis, Format.R16_UInt, 0);
            context.RSSetViewport(0, 0, TextureWidth, TextureHeight);
            context.OMSetRenderTargets(RenderTargetView!);
            context.IASetPrimitiveTopology(PrimitiveTopology.LineList);
            context.DrawIndexed(10, 0, 0);
        }

        void DrawSkeleton() {
            SkeletonPointsShader.Instance.Use(context);
            context.VSSetConstantBuffer(0, cb0);
            context.GSSetConstantBuffer(0, cb0);
            context.WriteBuffer(cb_bone, Skeleton.Instance.SelectedBone?.Index ?? -1);
            context.VSSetConstantBuffer(1, cb_bone);
            context.IASetVertexBuffer(0, Skeleton.Instance.BoneVertexBuffer, (uint)Marshal.SizeOf<Vector3>());
            context.RSSetViewport(0, 0, TextureWidth, TextureHeight);
            context.OMSetRenderTargets(RenderTargetView!);
            context.IASetPrimitiveTopology(PrimitiveTopology.PointList);
            context.Draw((uint)Skeleton.Instance.Bones.Count, 0);
            context.ClearState();
            SkeletonLinesShader.Instance.Use(context);
            context.VSSetConstantBuffer(0, cb0);
            context.IASetVertexBuffer(0, Skeleton.Instance.BoneVertexBuffer, (uint)Marshal.SizeOf<Vector3>());
            context.RSSetViewport(0, 0, TextureWidth, TextureHeight);
            context.OMSetRenderTargets(RenderTargetView!);
            context.IASetIndexBuffer(Skeleton.Instance.LineIndexBuffer, Format.R16_UInt, 0);
            context.IASetPrimitiveTopology(PrimitiveTopology.LineList);
            context.DrawIndexed(Skeleton.Instance.LineIndexBuffer.Description.ByteWidth / 2, 0, 0);
        }

        BeginDraw();
        context.ClearRenderTargetView(RenderTargetView, new(0xffffffff));
        context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        context.ClearState();
        DrawCharacter();
        context.ClearState();
        DrawAxis();
        context.ClearState();
        DrawSkeleton();
        needRedraw = false;
        return EndDraw();
    }

    private ConstantBuffer0Struct cb0_data = new();
    struct ConstantBuffer0Struct {
        public Matrix4x4 wvp;
        public int rtwidth;
        public int rtheight;
    }
    private void UpdateConstantBuffer0() {
        ID3D11DeviceContext context = DXObjects.D3D11Device.ImmediateContext;
        cb0_data.wvp = GetWorldViewProjection();
        cb0_data.rtheight = TextureHeight;
        cb0_data.rtwidth = TextureWidth;
        context.WriteBuffer(cb0, cb0_data);

        Matrix4x4 GetWorldViewProjection() {
            ID3D11DeviceContext context = DXObjects.D3D11Device.ImmediateContext;
            var mat = Matrix4x4.Identity;

            mat *= Matrix4x4.CreateTranslation(-cameraPosition);
            mat *= Matrix4x4.CreateRotationZ(-cameraYaw);
            mat *= Matrix4x4.CreateRotationX(-cameraPitch);

            var mat_swapyz = Matrix4x4.Identity;
            mat_swapyz.M22 = 0;
            mat_swapyz.M23 = 1;
            mat_swapyz.M32 = 1;
            mat_swapyz.M33 = 0;
            mat *= mat_swapyz;

            var mat_pers = Matrix4x4.Identity;
            mat_pers.M34 = 1;
            mat *= mat_pers;

            return mat;
        }
    }

    private Vector3 focusPoint = new(0, 0, 0);
    private float cameraYaw = 0f;
    private float cameraPitch = 0f;
    private float cameraDistance = 40f;
    private Vector3 cameraPosition = new(0, -40, 0);
    private bool isLeftPointerPressed = false;
    private bool isMiddlePointerPressed = false;
    private Point lastPointerPosition;
    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        isLeftPointerPressed = e.Properties.IsLeftButtonPressed;
        isMiddlePointerPressed = e.Properties.IsMiddleButtonPressed;
        lastPointerPosition = e.GetPosition(this);
    }
    protected override void OnPointerMoved(PointerEventArgs e) {
        Point currentPointerPosition = e.GetPosition(this);
        var delta = currentPointerPosition - lastPointerPosition;
        if (isLeftPointerPressed) {
            Debug.WriteLine(lastPointerPosition);
            Debug.WriteLine(currentPointerPosition);
            cameraPitch -= (float)(delta.Y * 0.005d);
            cameraPitch = Math.Clamp(cameraPitch, -MathF.PI / 2, MathF.PI / 2);
            cameraYaw -= (float)(delta.X * 0.005d);
            UpdateCameraPosition();
            needRedraw = true;
        }
        if (isMiddlePointerPressed) {
            focusPoint.X -= MathF.Cos(cameraYaw) * (float)delta.X * 0.1f;
            focusPoint.Y -= MathF.Sin(cameraYaw) * (float)delta.X * 0.1f;
            focusPoint.Z += MathF.Cos(cameraPitch) * (float)delta.Y * 0.1f;
            float distance_xy = MathF.Sin(cameraPitch) * (float)delta.Y * 0.1f;
            focusPoint.X += MathF.Sin(cameraYaw) * distance_xy;
            focusPoint.Y -= MathF.Cos(cameraYaw) * distance_xy;
            UpdateCameraPosition();
            needRedraw = true;
        }
        lastPointerPosition = currentPointerPosition;
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        isLeftPointerPressed = e.Properties.IsLeftButtonPressed;
        isMiddlePointerPressed = e.Properties.IsMiddleButtonPressed;
    }
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e) {
        cameraDistance -= (float)(e.Delta.Y * 5);
        UpdateCameraPosition();
        needRedraw = true;
    }

    private void UpdateCameraPosition() {
        cameraPosition = new(
            focusPoint.X + cameraDistance * MathF.Sin(cameraYaw) * MathF.Cos(cameraPitch),
            focusPoint.Y + cameraDistance * -MathF.Cos(cameraYaw) * MathF.Cos(cameraPitch),
            focusPoint.Z + cameraDistance * MathF.Sin(-cameraPitch)
        );
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);
        needRedraw = true;
    }
}
