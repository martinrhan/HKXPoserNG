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
        IObservable<int> observable_currentFrame =
            Animation.Instance.GetPropertyValueObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame);

        observable_selectedBone.Subscribe(_ => UpdateControlValues());

        Observable.CombineLatest(observable_selectedBone, observable_currentFrame, (b, f) => (b, f)).Subscribe(_ => UpdateControlValues());

        IObservable<bool> observable_hasKeyFrame =
            AnimationEditor.Instance.GetPropertyValueObservable(nameof(AnimationEditor.SelectedKeyFrame), ae => ae.SelectedKeyFrame != null);
        IObservable<AnimationModificationTrack?> observable_selectedModificationTrack =
            AnimationEditor.Instance.GetPropertyValueObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack);
        IObservable<IAvaloniaReadOnlyList<Bone>?> observable_affectedBones =
            observable_selectedModificationTrack.Select(smt => smt?.AffectedBones);
        IObservable<NotifyCollectionChangedEventArgs?> observable_collectionChanged = observable_selectedModificationTrack.
            Select(smt => smt?.AffectedBones.GetCollectionChangedObservable().
            Select(ep => ep.EventArgs) ?? Observable.Return<NotifyCollectionChangedEventArgs?>(null)).
            Switch();
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
        void AddFloat(string mark, Func<Bone?, float> getNewValue, Action<Bone, float>? setNewValue = null, Action<SliderCoveredNumberBox>? action = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            SliderCoveredNumberBox numberBox = new() {
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = 1,
                [IsEnabledProperty.Bind()] = observable_isSelectedBoneEditable.ToBinding(),
                ReadOnlyMode = setNewValue == null,
                Margin = new(10, 0, 0, 0),
            };
            if (setNewValue != null) numberBox.NumberChanged.Subscribe(t => {
                if (Skeleton.Instance.SelectedBone != null) setNewValue(Skeleton.Instance.SelectedBone, (float)t.NewValue);
            });
            action?.Invoke(numberBox);
            Action updateControlValueAction = () => numberBox.Number = getNewValue(Skeleton.Instance.SelectedBone);
            updateControlValueActions.Add(updateControlValueAction);
            grid.Children.Add(numberBox);
            row++;
        }
        void AddVector3(string mark, Func<Bone?, Vector3> getNewValue, Action<Bone, Vector3>? setNewValue = null, Action<Vector3Editor>? action = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            Vector3Editor vector3Editor = new() {
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = 1,
                [IsEnabledProperty.Bind()] = observable_isSelectedBoneEditable.ToBinding(),
                ReadOnlyMode = setNewValue == null,
                Margin = new(5, 0, 0, 0),
            };
            if (setNewValue != null) vector3Editor.VectorChanged.Subscribe(v => {
                if (Skeleton.Instance.SelectedBone != null) setNewValue(Skeleton.Instance.SelectedBone!, v);
            });
            action?.Invoke(vector3Editor);
            updateControlValueActions.Add(() => vector3Editor.Vector = getNewValue(Skeleton.Instance.SelectedBone));
            grid.Children.Add(vector3Editor);
            row++;
        }
        void AddEmptyRow() {
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) });
            row++;
        }
        AddHeader("Local Original");
        AddFloat("S", b => b?.LocalOriginal.Scale ?? 1);
        AddVector3("R", b => b?.LocalOriginal.Rotation.ToEuler() ?? Vector3.Zero, null, v3e => { v3e.Factor = Math.PI; v3e.FactorSymbol = "¦Ð"; });
        AddVector3("T", b => b?.LocalOriginal.Translation ?? Vector3.Zero);
        AddEmptyRow();
        AddHeader("Local Modification");
        AddFloat("S",
            b => b?.LocalModification.Scale ?? 1,
            (b, s) => b.LocalModification = b.LocalModification with { Scale = s },
            scnb => { scnb.Sensitivity = 0.01d; scnb.MinNumber = 0; scnb.MaxNumber = 1; }
        );
        AddVector3("R",
            b => b?.LocalModification.Rotation.ToEuler() ?? Vector3.Zero,
            (b, r) => b.LocalModification = b.LocalModification with { Rotation = r.FromEuler() },
            v3e => { v3e.Sensitivity = 0.01d; v3e.MinNumber = -Math.PI; v3e.MaxNumber = v3e.Factor = Math.PI; v3e.FactorSymbol = "¦Ð"; }
        );
        AddVector3("T", b => b?.LocalModification.Translation ?? Vector3.Zero, (b, t) => b.LocalModification = b.LocalModification with { Translation = t });
        AddEmptyRow();
        AddHeader("Local Modified");
        AddFloat("S", b => b?.LocalModified.Scale ?? 1);
        AddVector3("R", b => b?.LocalModified.Rotation.ToEuler() ?? Vector3.Zero, null, v3e => { v3e.Factor = Math.PI; v3e.FactorSymbol = "¦Ð"; });
        AddVector3("T", b => b?.LocalModified.Translation ?? Vector3.Zero);
    }

    private void UpdateControlValues() {
        foreach (var action in updateControlValueActions) {
            action();
        }
    }
    private List<Action> updateControlValueActions = new();
}