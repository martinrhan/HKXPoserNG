using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HKXPoserNG.Controls;
using HKXPoserNG.Mvvm;
using HKXPoserNG.Reactive;
using HKXPoserNG.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using Vortice.Mathematics;

namespace HKXPoserNG.Views;

public partial class SelectedBoneView : UserControl {
    public static SimpleValueConverter<Quaternion, Vector3> QuaternionVector3Converter { get; } = new(
        q => q.ToEuler(),
        v => Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z)
        );

    public SelectedBoneView() {
        InitializeComponent();
        IObservable<Bone?> observable_selectedBone =
            Skeleton.Instance.GetPropertyValueObservable(nameof(Skeleton.SelectedBone), s => s.SelectedBone);
        IObservable<bool> observable_hasKeyFrame =
            AnimationEditor.Instance.GetPropertyValueObservable(nameof(AnimationEditor.SelectedKeyFrame), ae => ae.SelectedKeyFrame != null);
        IObservable<AnimationModificationTrack?> observable_selectedModificationTrack =
            AnimationEditor.Instance.GetPropertyValueObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack);
        IObservable<IAvaloniaReadOnlyList<Bone>?> observable_affectedBones =
            observable_selectedModificationTrack.Select(smt => smt?.AffectedBones);
        IObservable<NotifyCollectionChangedEventArgs?> observable_collectionChanged = 
            observable_selectedModificationTrack
            .Select(smt => smt?.AffectedBones.GetCollectionChangedObservable()
            .Select(ep => ep.EventArgs) ?? Observable.Return<NotifyCollectionChangedEventArgs?>(null))
            .Switch();
        IObservable<bool> observable_isSelectedBoneEditable = Observable.CombineLatest(
            observable_hasKeyFrame, observable_selectedBone, observable_affectedBones, observable_collectionChanged,
            (hasKeyFrame, selectedBone, affectedBones, _) => {
                if (!hasKeyFrame || selectedBone == null || affectedBones == null) return false;
                return affectedBones.Contains(selectedBone);
            }
        );

        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

        int row = 0;
        void AddHeader(string text) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = text, FontWeight = FontWeight.Bold, [Grid.RowProperty] = row++, [Grid.ColumnSpanProperty] = 2 });
        }
        void AddDouble(string mark, Func<Bone?, IObservable<double>> getNewValueObservable, Action<Bone, double>? setNewValue = null, Action<SliderCoveredNumberBox>? action = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            SliderCoveredNumberBox numberBox = new() {
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = 1,
                [IsEnabledProperty.Bind()] = observable_isSelectedBoneEditable.ToBinding(),
                ReadOnlyMode = setNewValue == null,
                Margin = new(10, 0, 0, 0),
                [SliderCoveredNumberBox.NumberProperty.Bind()] = observable_selectedBone.Select(b => getNewValueObservable(b)).Switch().ToBinding(),
            };
            if (setNewValue != null) numberBox.NumberChanged.Subscribe(t => {
                if (Skeleton.Instance.SelectedBone != null) setNewValue(Skeleton.Instance.SelectedBone, t.NewValue);
            });
            action?.Invoke(numberBox);
            grid.Children.Add(numberBox);
            row++;
        }
        void AddVector3(string mark, Func<Bone?, IObservable<Vector3>> getNewValueObservable, Action<Bone, Vector3>? setNewValue = null, Action<Vector3Editor>? action = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            Vector3Editor vector3Editor = new() {
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = 1,
                [IsEnabledProperty.Bind()] = observable_isSelectedBoneEditable.ToBinding(),
                ReadOnlyMode = setNewValue == null,
                Margin = new(5, 0, 0, 0),
                [Vector3Editor.VectorProperty.Bind()] = observable_selectedBone.Select(b => getNewValueObservable(b)).Switch().ToBinding(),
            };
            if (setNewValue != null) vector3Editor.VectorChanged.Subscribe(v => {
                if (Skeleton.Instance.SelectedBone != null) setNewValue(Skeleton.Instance.SelectedBone!, v);
            });
            action?.Invoke(vector3Editor);
            grid.Children.Add(vector3Editor);
            row++;
        }
        void AddQuaternion(string mark, Func<Bone?, IObservable<Quaternion>> getNewValueObservable, Action<Bone, Quaternion>? setNewValue = null, Action<QuaternionEditor>? action = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            QuaternionEditor quaternionEditor = new() {
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = 1,
                [IsEnabledProperty.Bind()] = observable_isSelectedBoneEditable.ToBinding(),
                ReadOnlyMode = setNewValue == null,
                Margin = new(5, 0, 0, 0),
                [QuaternionEditor.QuaternionProperty.Bind()] = observable_selectedBone.Select(b => getNewValueObservable(b)).Switch().ToBinding(),
            };
            if (setNewValue != null) quaternionEditor.QuaternionChanged.Subscribe(q => {
                if (Skeleton.Instance.SelectedBone != null) setNewValue(Skeleton.Instance.SelectedBone!, q);
            });
            action?.Invoke(quaternionEditor);
            grid.Children.Add(quaternionEditor);
            row++;
        }
        void AddEmptyRow() {
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) });
            row++;
        }
        AddHeader("Local Original");
        AddDouble("S", 
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalOriginal),  b => (double)b.LocalOriginal.Scale) ?? Observable.Return(1d)
        );
        AddQuaternion("R",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalOriginal), b => b.LocalOriginal.Rotation) ?? Observable.Return(Quaternion.Identity)
        );
        AddVector3("T",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalOriginal), b => b.LocalOriginal.Translation) ?? Observable.Return(Vector3.Zero)
        );
        AddEmptyRow();
        AddHeader("Local Modification");
        AddDouble("S",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalModificationInSelectedTrack), b => (double)b.LocalModificationInSelectedTrack.Scale) ?? Observable.Return(1d),
            (b, s) => b.SetLocalModification(b.LocalModificationInSelectedTrack with { Scale = (float)s }),
            scnb => { scnb.Sensitivity = 0.01d; scnb.MinNumber = 0; scnb.MaxNumber = 1; }
        );
        AddQuaternion(
            "R",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalModificationInSelectedTrack), b => b.LocalModificationInSelectedTrack.Rotation) ?? Observable.Return(Quaternion.Identity),
            (b, r) => b.SetLocalModification(b.LocalModificationInSelectedTrack with { Rotation = r })
        );
        AddVector3("T",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalModificationInSelectedTrack), b => b.LocalModificationInSelectedTrack.Translation) ?? Observable.Return(Vector3.Zero),
            (b, t) => b.SetLocalModification(b.LocalModificationInSelectedTrack with { Translation = t })
        );
        AddEmptyRow();
        AddHeader("Local Modified");
        AddDouble("S", 
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalModified), b => (double)b.LocalModified.Scale) ?? Observable.Return(1d)
        );
        AddQuaternion(
            "R",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalModified), b => b.LocalModified.Rotation) ?? Observable.Return(Quaternion.Identity)
        );
        AddVector3("T",
            b => b?.GetPropertyValueObservable(nameof(Bone.LocalModified), b => b.LocalModified.Translation) ?? Observable.Return(Vector3.Zero)
        );
    }
}