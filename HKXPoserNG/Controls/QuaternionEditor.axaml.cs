using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DependencyPropertyGenerator;
using HKXPoserNG.Extensions;
using HKXPoserNG.Reactive;
using System;
using System.Numerics;

namespace HKXPoserNG.Controls;

[DependencyProperty("Quaternion", typeof(Quaternion), DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty("Sensitivity", typeof(double), DefaultValue = 0.01)]
[DependencyProperty("ReadOnlyMode", typeof(bool))]
public partial class QuaternionEditor : UserControl {
    public QuaternionEditor() {
        InitializeComponent();

        textBlockTheta.Text = "жи";
        numberBoxTheta[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxTheta[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxTheta[SliderCoveredNumberBox.MinNumberProperty] = -1;
        numberBoxTheta[SliderCoveredNumberBox.MaxNumberProperty] = 1;

        numberBoxX[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxX[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxX[SliderCoveredNumberBox.MinNumberProperty] = -1;
        numberBoxX[SliderCoveredNumberBox.MaxNumberProperty] = 1;

        numberBoxY[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxY[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxY[SliderCoveredNumberBox.MinNumberProperty] = -1;
        numberBoxY[SliderCoveredNumberBox.MaxNumberProperty] = 1;

        numberBoxZ[SliderCoveredNumberBox.SensitivityProperty.Bind()] = this[SensitivityProperty.Bind()];
        numberBoxZ[SliderCoveredNumberBox.ReadOnlyModeProperty.Bind()] = this[ReadOnlyModeProperty.Bind()];
        numberBoxZ[SliderCoveredNumberBox.MinNumberProperty] = -1;
        numberBoxZ[SliderCoveredNumberBox.MaxNumberProperty] = 1;

        Quaternion = Quaternion.Identity;

        numberBoxTheta.NumberChanged.Subscribe(NumberBoxTheta_NumberChanged);
        numberBoxX.NumberChanged.Subscribe(NumberBoxX_NumberChanged);
        numberBoxY.NumberChanged.Subscribe(NumberBoxY_NumberChanged);
        numberBoxZ.NumberChanged.Subscribe(NumberBoxZ_NumberChanged);
    }

    private bool isCallingNumberBoxThetaNumberChanged = false;
    private void NumberBoxTheta_NumberChanged(ValueChangedTuple<double> tuple) {
        if (isCallingNumberBoxThetaNumberChanged) return;
        isCallingNumberBoxThetaNumberChanged = true;
        double theta = numberBoxTheta.Number;
        double x = numberBoxX.Number;
        double y = numberBoxY.Number;
        double z = numberBoxZ.Number;
        Quaternion result = Quaternion.CreateFromAxisAngle(new((float)x, (float)y, (float)z), (float)theta);
        this.Quaternion = result;
        isCallingNumberBoxThetaNumberChanged = false;
    }

    private bool isCallingNumberBoxXNumberChanged = false;
    private void NumberBoxX_NumberChanged(ValueChangedTuple<double> tuple) {
        if (isCallingNumberBoxXNumberChanged) return;
        isCallingNumberBoxXNumberChanged = true;
        double y_old = numberBoxY.Number;
        double z_old = numberBoxZ.Number;
        double yz_old_magnitude = Math.Sqrt(y_old * y_old + z_old * z_old);
        double y_new, z_new;
        double x_new = numberBoxX.Number;
        double y2plusz2_new = 1 - x_new * x_new;
        if (Math.Abs(yz_old_magnitude) < 1e-6) {
            z_new = y_new = Math.Sqrt(y2plusz2_new / 2);
        } else {
            double yz_new_magnitude = Math.Sqrt(y2plusz2_new);
            double scale_for_yz = yz_new_magnitude / yz_old_magnitude;
            y_new = y_old * scale_for_yz;
            z_new = z_old * scale_for_yz;
        }
        numberBoxY.Number = y_new;
        numberBoxZ.Number = z_new;
        Quaternion result = Quaternion.CreateFromAxisAngle(new((float)x_new, (float)y_new, (float)z_new), (float)numberBoxTheta.Number);
        this.Quaternion = result;
        isCallingNumberBoxXNumberChanged = false;
    }

    private bool isCallingNumberBoxYNumberChanged = false;
    private void NumberBoxY_NumberChanged(ValueChangedTuple<double> tuple) {
        if (isCallingNumberBoxYNumberChanged) return;
        isCallingNumberBoxYNumberChanged = true;
        double x_old = numberBoxX.Number;
        double z_old = numberBoxZ.Number;
        double xz_old_magnitude = Math.Sqrt(x_old * x_old + z_old * z_old);
        double x_new, z_new;
        double y_new = numberBoxY.Number;
        double x2plusz2_new = 1 - y_new * y_new;
        if (Math.Abs(xz_old_magnitude) < 1e-9) {
            x_new = Math.Sqrt(x2plusz2_new / 2);
            z_new = x_new;
        } else {
            double xz_new_magnitude = Math.Sqrt(x2plusz2_new);
            double scale_for_xz = xz_new_magnitude / xz_old_magnitude;
            x_new = x_old * scale_for_xz;
            z_new = z_old * scale_for_xz;
        }
        numberBoxX.Number = x_new;
        numberBoxZ.Number = z_new;
        Quaternion result = Quaternion.CreateFromAxisAngle(new((float)x_new, (float)y_new, (float)z_new), (float)numberBoxTheta.Number);
        this.Quaternion = result;
        isCallingNumberBoxYNumberChanged = false;
    }

    private bool isCallingNumberBoxZNumberChanged = false;
    private void NumberBoxZ_NumberChanged(ValueChangedTuple<double> tuple) {
        if (isCallingNumberBoxZNumberChanged) return;
        double x_old = numberBoxX.Number;
        double y_old = numberBoxY.Number;
        double xy_old_magnitude = Math.Sqrt(x_old * x_old + y_old * y_old);
        double x_new, y_new;
        double z_new = numberBoxZ.Number;
        double x2plusy2_new = 1 - z_new * z_new;
        if (Math.Abs(xy_old_magnitude) < 1e-9) {
            x_new = Math.Sqrt(x2plusy2_new / 2);
            y_new = x_new;
        } else {
            double xy_new_magnitude = Math.Sqrt(x2plusy2_new);
            double scale_for_xy = xy_new_magnitude / xy_old_magnitude;
            x_new = x_old * scale_for_xy;
            y_new = y_old * scale_for_xy;
        }
        numberBoxX.Number = x_new;
        numberBoxY.Number = y_new;
        Quaternion result = Quaternion.CreateFromAxisAngle(new((float)x_new, (float)y_new, (float)z_new), (float)numberBoxTheta.Number);
        this.Quaternion = result;
        isCallingNumberBoxZNumberChanged = false;
    }

    private bool isCallingOnQuaternionChanged = false;
    partial void OnQuaternionChanged() {
        if (isCallingOnQuaternionChanged) return;
        isCallingOnQuaternionChanged = true;
        (Vector3 axis, float angle) = this.Quaternion.ToAxisAngle();
        numberBoxTheta.Number = angle;
        numberBoxX.Number = axis.X;
        numberBoxY.Number = axis.Y;
        numberBoxZ.Number = axis.Z;
        quaternionChanged.Notify(this.Quaternion);
        isCallingOnQuaternionChanged = false;
    }

    private SimpleObservable<Quaternion> quaternionChanged = new();
    public IObservable<Quaternion> QuaternionChanged => quaternionChanged;
}