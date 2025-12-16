using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DependencyPropertyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Data;
using System.Reactive.Linq;
using Avalonia.Data.Converters;
using HKXPoserNG.Reactive;

namespace HKXPoserNG.Controls;

[DependencyProperty("Number", typeof(double))]
[DependencyProperty("MinNumber", typeof(double))]
[DependencyProperty("MaxNumber", typeof(double), DefaultValue = 100)]
[DependencyProperty("Sensitivity", typeof(double), DefaultValue = 1)]
public partial class SliderCoveredNumberBox : Panel {
    public SliderCoveredNumberBox() {
        textBox = new() {
            Text = Number.ToString(),
            Padding = new(0),
        };
        textBox.LostFocus += TextBox_LostFocus;
        sliderCover = new() {
            Cursor = new(StandardCursorType.SizeWestEast),
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
        };
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<TextBox>((sender, e) => {
            if (sender != textBox) { return; }
            sliderCover.IsHitTestVisible = !textBox.IsKeyboardFocusWithin;
        });
        sliderCover.PointerPressed += SliderCover_PointerPressed;
        sliderCover.PointerReleased += SliderCover_PointerReleased;
        sliderCover.PointerMoved += SliderCover_PointerMoved;
        Children.Add(textBox);
        Children.Add(sliderCover);
    }

    partial void OnNumberChanged(double oldValue, double newValue) {
        numberChanged.Notify(new (oldValue, newValue));
    }

    TextBox textBox;
    Panel sliderCover;

    private void TextBox_LostFocus(object? sender, RoutedEventArgs e) {
        double number;
        if (double.TryParse(textBox.Text, out number)) {
            Number = number;
        }
    }
    partial void OnNumberChanged(double newValue) {
        textBox.Text = newValue.ToString();
    }

    private Stopwatch pointerPressedStopwatch = new();
    private bool pointerPressed = false;
    private Point lastPointerPosition;
    private void SliderCover_PointerPressed(object? sender, PointerPressedEventArgs e) {
        pointerPressedStopwatch.Restart();
        pointerPressed = true;
        lastPointerPosition = e.GetPosition(sliderCover);
    }
    private void SliderCover_PointerReleased(object? sender, PointerReleasedEventArgs e) {
        if (pointerPressedStopwatch.ElapsedMilliseconds < 500) {
            textBox.Focus();
        }
        pointerPressedStopwatch.Stop();
        pointerPressed = false;
    }
    private void SliderCover_PointerMoved(object? sender, PointerEventArgs e) {
        if (pointerPressed) {
            Point pointerPosotion = e.GetPosition(sliderCover);
            double delta = pointerPosotion.X - lastPointerPosition.X;
            Number = Math.Clamp(Number + delta * Sensibility, MinNumber, MaxNumber);
            lastPointerPosition = pointerPosotion;
        }
    }

    private SimpleObservable<ValueChangedTuple<double>> numberChanged = new();
    public IObservable<ValueChangedTuple<double>> NumberChanged => numberChanged;
}

