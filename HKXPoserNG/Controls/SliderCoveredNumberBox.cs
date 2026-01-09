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
public partial class SliderCoveredNumberBox : Panel {
    public SliderCoveredNumberBox() {
        textBox = new() {
            Text = Number.ToString(),
            Padding = new(0),
        };
        textBox.LostFocus += TextBox_LostFocus;
        Children.Add(textBox);
        sliderCover = new() {
            [CursorProperty.Bind()] = this.GetObservable(ReadOnlyModeProperty).Select<bool, Cursor?>(b => b ? null : new(StandardCursorType.SizeWestEast)).ToBinding(),
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
    Panel sliderCover;

    bool isCallingSetNumberFromTextBox = false;
    private void SetNumberFromTextBox() {
        double number_text;
        if (double.TryParse(textBox.Text, out number_text)) {
            isCallingSetNumberFromTextBox = true;
            Number = Math.Clamp(number_text, MinNumber, MaxNumber);
            isCallingSetNumberFromTextBox = false;
        }
    }
    private void TextBox_LostFocus(object? sender, RoutedEventArgs e) {
        SetNumberFromTextBox();
    }
    partial void OnNumberChanged(double oldValue, double newValue) {
        if (!isCallingSetNumberFromTextBox)
            textBox.Text = newValue.ToString("F3");
        numberChanged.Notify(new(oldValue, newValue));
    }

    private bool pointerPressed = false;
    private Point lastPointerPosition;
    private void SliderCover_PointerPressed(object? sender, PointerPressedEventArgs e) {
        if (ReadOnlyMode) return;
        this.Focus();
        pointerPressed = true;
        lastPointerPosition = e.GetPosition(sliderCover);
    }
    bool sliderMoved = false;
    private void SliderCover_PointerReleased(object? sender, PointerReleasedEventArgs e) {
        if (ReadOnlyMode) return;
        if (sliderMoved) {
            SetNumberFromTextBox();
            sliderMoved = false;
        } else {
            textBox.Focus();
        }
        pointerPressed = false;
    }
    private void SliderCover_PointerMoved(object? sender, PointerEventArgs e) {
        if (ReadOnlyMode) return;
        if (pointerPressed) {
            double number_text;
            if (!double.TryParse(textBox.Text, out number_text)) return;
            Point pointerPosotion = e.GetPosition(sliderCover);
            double delta = pointerPosotion.X - lastPointerPosition.X;
            number_text = Math.Clamp(number_text + delta * Sensitivity, MinNumber, MaxNumber);
            textBox.Text = number_text.ToString("F3");
            sliderMoved = true;
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

