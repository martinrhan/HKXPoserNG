using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DependencyPropertyGenerator;
using HKXPoserNG.Controls;
using HKXPoserNG.Reactive;
using System;
using System.Numerics;

namespace HKXPoserNG;

[DependencyProperty("Vector", typeof(Vector3), DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty("Sensitivity", typeof(double), DefaultValue = 1)]
[DependencyProperty("ReadOnlyMode", typeof(bool))]
[DependencyProperty("MinNumber", typeof(double), DefaultValue = double.NegativeInfinity)]
[DependencyProperty("MaxNumber", typeof(double), DefaultValue = double.PositiveInfinity)]
public partial class Vector3Editor : UserControl {
    public Vector3Editor() {
        InitializeComponent();

        numberBoxX[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxX[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxX[SliderCoveredNumberBox.MinNumberProperty.Bind()] = this[MinNumberProperty.Bind()];
        numberBoxX[SliderCoveredNumberBox.MaxNumberProperty.Bind()] = this[MaxNumberProperty.Bind()];

        numberBoxY[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxY[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxY[SliderCoveredNumberBox.MinNumberProperty.Bind()] = this[MinNumberProperty.Bind()];
        numberBoxY[SliderCoveredNumberBox.MaxNumberProperty.Bind()] = this[MaxNumberProperty.Bind()];

        numberBoxZ[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxZ[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxZ[SliderCoveredNumberBox.MinNumberProperty.Bind()] = this[MinNumberProperty.Bind()];
        numberBoxZ[SliderCoveredNumberBox.MaxNumberProperty.Bind()] = this[MaxNumberProperty.Bind()];

        numberBoxX.PropertyChanged += NumberBoxX_PropertyChanged;
        numberBoxY.PropertyChanged += NumberBoxY_PropertyChanged;
        numberBoxZ.PropertyChanged += NumberBoxZ_PropertyChanged; 
    }

    private void NumberBoxX_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (isCallingOnVectorChanged) { return; }
        this.Vector = this.Vector with { X = (float)numberBoxX.Number };
    }
    private void NumberBoxY_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (isCallingOnVectorChanged) { return; }
        this.Vector = this.Vector with { Y = (float)numberBoxY.Number };
    }
    private void NumberBoxZ_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (isCallingOnVectorChanged) { return; }
        this.Vector = this.Vector with { Z = (float)numberBoxZ.Number };
    }

    private bool isCallingOnVectorChanged = false;
    partial void OnVectorChanged(Vector3 newValue) {
        isCallingOnVectorChanged = true;
        numberBoxX.Number = newValue.X;
        numberBoxY.Number = newValue.Y;
        numberBoxZ.Number = newValue.Z;
        vectorChanged.Notify(newValue); 
        isCallingOnVectorChanged = false;
    }

    private SimpleObservable<Vector3> vectorChanged = new ();
    public IObservable<Vector3> VectorChanged => vectorChanged;
}