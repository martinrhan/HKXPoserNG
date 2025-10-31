using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HKXPoserNG.Controls;
using HKXPoserNG.Mvvm;
using HKXPoserNG.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
                updateControlValueAction = () => numberBox.Number =  getNewValue(Skeleton.Instance.SelectedBone);
            }
            updateControlValueActions.Add(updateControlValueAction);
            grid.Children.Add(control);
            row++;
        }
        void AddVector3ReadOnly(string mark, Func<Bone?, Vector3> getNewValue) {
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(new TextBlock() { Text = mark, [Grid.RowProperty] = row });
            Vector3Displayer vector3Displayer = new() { [Grid.RowProperty] = row, [Grid.ColumnProperty] = 1 };
            grid.Children.Add(vector3Displayer);
            updateControlValueActions.Add(() => {
                vector3Displayer.Vector = getNewValue(Skeleton.Instance.SelectedBone);
            });
            row++;
        }
        AddHeader("Local Original");
        AddFloat("S", bone => bone?.LocalOriginal.Scale ?? 1);
        AddVector3ReadOnly("R", bone => bone?.LocalOriginal.Rotation.ToEuler() ?? Vector3.Zero);
        AddVector3ReadOnly("T", bone => bone?.LocalOriginal.Translation ?? Vector3.Zero);
        AddHeader("Local Modification");
        AddFloat("S", bone => bone?.LocalModification.Scale ?? 1, (b, s) => b.LocalModification = b.LocalModification with { Scale = s });
    }

    private void UpdateControlValues() {
        foreach (var action in updateControlValueActions) {
            action();
        }
    }
    private List<Action> updateControlValueActions = new();
}