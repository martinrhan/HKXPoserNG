using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
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

public partial class BoneView : UserControl {
    public static SimpleValueConverter<Quaternion, Vector3> QuaternionVector3Converter { get; } = new(
        q => q.ToEuler(),
        v => Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z)
        );

    public BoneView() {
        Skeleton.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(Skeleton.SelectedBone)) {
                this.DataContext = Skeleton.Instance.SelectedBone;
                UpdateControlValues();
            }
        };
        this.DataContext = Skeleton.Instance.SelectedBone;
        InitializeComponent();

        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

        int row = 0;
        void AddHeader(string text) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = text, [Grid.RowProperty] = row++, [Grid.ColumnSpanProperty] = 2 });
        }
        void AddFloat(string mark, Func<Bone?, float> getNewValue, Action<Bone, float>? setNewValue = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            Control control;
            Action updateControlValueAction;
            if (setNewValue == null) {
                TextBlock textBlock = new() { [Grid.RowProperty] = row, [Grid.ColumnProperty] = 1 };
                control = textBlock;
                updateControlValueAction = () => textBlock.Text = getNewValue(Skeleton.Instance.SelectedBone).ToString();
            } else {
                SliderCoveredNumberBox numberBox = new() { [Grid.RowProperty] = row, [Grid.ColumnProperty] = 1 };
                control = numberBox;
                numberBox.NumberChanged.Subscribe(t => {
                    if (Skeleton.Instance.SelectedBone != null)
                        setNewValue(Skeleton.Instance.SelectedBone, (float)t.NewValue);
                });
                updateControlValueAction = () => numberBox.Number = getNewValue(Skeleton.Instance.SelectedBone);
            }
            updateControlValueActions.Add(updateControlValueAction);
            grid.Children.Add(control);
            row++;
        }
        void AddVector3(string mark, Func<Bone?, Vector3> getNewValue, Action<Bone, Vector3>? setNewValue = null) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            if (setNewValue == null) {
                Vector3Displayer vector3Displayer = new() { [Grid.RowProperty] = row, [Grid.ColumnProperty] = 1 };
                updateControlValueActions.Add(() => {
                    vector3Displayer.Vector = getNewValue(Skeleton.Instance.SelectedBone);
                });
                grid.Children.Add(vector3Displayer);
            } else {
                IObservable<bool> observable_hasKeyFrame =
                    AnimationEditor.Instance.GetPropertyObservable(nameof(AnimationEditor.SelectedKeyFrame), ae => ae.SelectedKeyFrame != null);
                IObservable<Bone?> observable_selectedBone = 
                    Skeleton.Instance.GetPropertyObservable(nameof(Skeleton.SelectedBone), s => s.SelectedBone);
                IObservable<AnimationModificationTrack?> observable_selectedModificationTrack = 
                    AnimationEditor.Instance.GetPropertyObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack);
                IObservable<AvaloniaList<Bone>?> observable_affectedBones = 
                    observable_selectedModificationTrack.Select(smt => smt?.AffectedBones);
                IObservable<NotifyCollectionChangedEventArgs?> observable_collectionChanged = observable_selectedModificationTrack.
                    Select(smt => smt?.AffectedBones.GetObservable().
                    Select(ep => ep.EventArgs) ?? Observable.Return<NotifyCollectionChangedEventArgs?>(null)).
                    Switch();
                IObservable<bool> observable_isAffected = Observable.CombineLatest(
                    observable_selectedBone, observable_affectedBones, observable_collectionChanged,
                    (selectedBone, affectedBones, _) => {
                        if (selectedBone == null || affectedBones == null) return false;
                        return affectedBones.Contains(selectedBone);
                    }
                );
                Vector3Editor vector3Editor = new() {
                    [Grid.RowProperty] = row,
                    [Grid.ColumnProperty] = 1,
                    [IsEnabledProperty.Bind()] = observable_isAffected.ToBinding()
                };
                //observable_isAffected.Subscribe(b => vector3Editor.IsEnabled = b);
                vector3Editor.VectorChanged.Subscribe(v => {

                });
                updateControlValueActions.Add(() => {
                    vector3Editor.Vector = getNewValue(Skeleton.Instance.SelectedBone);
                });
                grid.Children.Add(vector3Editor);
            }
            row++;
        }
        AddHeader("Local Original");
        AddFloat("S", b => b?.LocalOriginal.Scale ?? 1);
        AddVector3("R", b => b?.LocalOriginal.Rotation.ToEuler() ?? Vector3.Zero);
        AddVector3("T", b => b?.LocalOriginal.Translation ?? Vector3.Zero);
        AddHeader("Local Modification");
        AddFloat("S", b => b?.LocalModification.Scale ?? 1, (b, s) => b.LocalModification = b.LocalModification with { Scale = s });
        AddVector3("R", b => b?.LocalModification.Rotation.ToEuler() ?? Vector3.Zero, (b, r) => b.LocalModification = b.LocalModification with { Rotation = r.FromEuler() });
        AddVector3("T", b => b?.LocalModification.Translation ?? Vector3.Zero, (b, t) => b.LocalModification = b.LocalModification with { Translation = t });
    }

    private void UpdateControlValues() {
        foreach (var action in updateControlValueActions) {
            action();
        }
    }
    private List<Action> updateControlValueActions = new();
}