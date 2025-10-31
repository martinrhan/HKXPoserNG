using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DependencyPropertyGenerator;
using HKXPoserNG.Controls;
using System.Numerics;

namespace HKXPoserNG;

[DependencyProperty("Vector", typeof(Vector3), DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty("Sensibility", typeof(double), DefaultValue = 1)]
public partial class Vector3Editor : UserControl {
    public Vector3Editor() {
        InitializeComponent();
        numberBoxX[SliderCoveredNumberBox.SensibilityProperty.Bind()] = this[SensibilityProperty.Bind()];
        numberBoxY[SliderCoveredNumberBox.SensibilityProperty.Bind()] = this[SensibilityProperty.Bind()];
        numberBoxZ[SliderCoveredNumberBox.SensibilityProperty.Bind()] = this[SensibilityProperty.Bind()];

        numberBoxX.PropertyChanged += NumberBoxX_PropertyChanged;
        numberBoxY.PropertyChanged += NumberBoxY_PropertyChanged;
        numberBoxZ.PropertyChanged += NumberBoxZ_PropertyChanged;
    }

    private void NumberBoxX_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        this.Vector = this.Vector with { X = (float)numberBoxX.Number };
    }
    private void NumberBoxY_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        this.Vector = this.Vector with { Y = (float)numberBoxY.Number };
    }
    private void NumberBoxZ_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        this.Vector = this.Vector with { Z = (float)numberBoxZ.Number };
    }

    partial void OnVectorChanged(Vector3 newValue) {
        numberBoxX.Number = newValue.X;
        numberBoxY.Number = newValue.Y;
        numberBoxZ.Number = newValue.Z;
    }
}