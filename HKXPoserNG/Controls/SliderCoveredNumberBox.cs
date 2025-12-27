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
using Avalonia.Controls.Templates;
using Avalonia.Controls.Presenters;

namespace HKXPoserNG.Controls;

[DependencyProperty("Number", typeof(double))]
[DependencyProperty("MinNumber", typeof(double), DefaultValue = double.NegativeInfinity)]
[DependencyProperty("MaxNumber", typeof(double), DefaultValue = double.PositiveInfinity)]
[DependencyProperty("Sensitivity", typeof(double), DefaultValue = 1)]
[DependencyProperty("ReadOnlyMode", typeof(bool))]
[DependencyProperty("Factor", typeof(double), DefaultValue = 1)]
[DependencyProperty("FactorSymbol", typeof(string))]
public partial class SliderCoveredNumberBox : Grid {
    public SliderCoveredNumberBox() {
        ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)];
        textBox = new() {
            Text = Number.ToString(),
            Padding = new(0),
        };
        textBox.LostFocus += TextBox_LostFocus;
        Children.Add(textBox);
        textBlock = new() {
            Text = FactorSymbol,
            Margin = new(0,0,1,0),
            [Grid.ColumnProperty] = 1,
        };
        Children.Add(textBlock);
        sliderCover = new() {
            Cursor = new(StandardCursorType.SizeWestEast),
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
            [Grid.ColumnSpanProperty] = 2,
        };
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<TextBox>((sender, e) => {
            if (sender != textBox) { return; }
            sliderCover.IsHitTestVisible = !textBox.IsKeyboardFocusWithin;
        });
        sliderCover.PointerPressed += SliderCover_PointerPressed;
        sliderCover.PointerReleased += SliderCover_PointerReleased;
        sliderCover.PointerMoved += SliderCover_PointerMoved;
        Children.Add(sliderCover);
    }

    TextBox textBox;
    TextBlock textBlock;
    Panel sliderCover;

    private void TextBox_LostFocus(object? sender, RoutedEventArgs e) {
        double coeifficient;
        if (double.TryParse(textBox.Text, out coeifficient)) {
            Number = Math.Clamp(coeifficient * Factor, MinNumber, MaxNumber);
        }
    }
    partial void OnNumberChanged(double oldValue, double newValue) {
        numberChanged.Notify(new(oldValue, newValue));
        textBox.Text = (newValue / Factor).ToString("F3");
    }
    partial void OnFactorChanged(double newValue) {
        textBox.Text = (Number / newValue).ToString();
    }
    partial void OnFactorSymbolChanged(string? newValue) {
        textBlock.Text = newValue;
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
            Number = Math.Clamp(Number + delta * Sensitivity, MinNumber, MaxNumber);
            lastPointerPosition = pointerPosotion;
        }
    }

    private SimpleObservable<ValueChangedTuple<double>> numberChanged = new();
    public IObservable<ValueChangedTuple<double>> NumberChanged => numberChanged;

    private FuncControlTemplate<TextBox> readOnlyTemplate = new((c, ns) => {
        TextPresenter textPresenter = new() {
            Name = "PART_TextPresenter",
            [TextPresenter.TextProperty.Bind()] = new Binding() {
                Path = "Text", RelativeSource = new() { Mode = RelativeSourceMode.TemplatedParent }
            }
        };
        ns.Register("PART_TextPresenter", textPresenter);
        return textPresenter;
    });
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        var defaultTemplate = textBox.Template;
        textBox[TemplatedControl.TemplateProperty.Bind()] = this.
            GetObservable(ReadOnlyModeProperty).
            Select(readOnlyMode => readOnlyMode ? readOnlyTemplate : defaultTemplate).
            ToBinding();
    }
}

