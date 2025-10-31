using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using DependencyPropertyGenerator;
using HKXPoserNG.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace HKXPoserNG.Views;

[DependencyProperty("TrackNameColumnWidth", typeof(double), DefaultValue = 100)]
[DependencyProperty("TrackRowHeight", typeof(double), DefaultValue = 30)]
public partial class AnimationEditorView : UserControl {
    public AnimationEditorView() {
        this.DataContext = AnimationEditor.Instance;
        InitializeComponent();
        canvas_frameIndicator.PointerPressed += Canvas_FrameIndicator_PointerPressed;
        canvas_frameIndicator.PointerMoved += Canvas_FrameIndicator_PointerMoved;
        canvas_frameIndicator.PointerReleased += Canvas_FrameIndicator_PointerReleased;
        canvas_frameIndicator.SizeChanged += Canvas_FrameIndicator_SizeChanged;
        currentFrameIndicator = new Panel {
            Background = Brushes.Red,
            Width = 1,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        canvas_frameIndicator.Children.Add(currentFrameIndicator);
        Animation.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(Animation.CurrentFrame)) {
                currentFrameIndicator[Canvas.LeftProperty] = GetLeftForFrameIndex(Animation.Instance.CurrentFrame);
            }
        };
    }

    private Panel currentFrameIndicator;

    private int MaxFrameIndex => Animation.Instance.FrameCount - 1;
    public double GetLeftForFrameIndex(int frameIndex) {
        return frameIndex / (double)MaxFrameIndex * canvas_frameIndicator.Bounds.Width;
    }

    private int GetFrameIndexFromPosition(double x) {
        return int.Clamp((int)Math.Round(x / canvas_frameIndicator.Bounds.Width * MaxFrameIndex), 0, MaxFrameIndex);
    }

    private bool isPointerPressed = false;
    private void Canvas_FrameIndicator_PointerPressed(object? sender, PointerPressedEventArgs e) {
        if (Animation.Instance.FrameCount == 0) return;
        Point p = e.GetPosition(canvas_frameIndicator);
        Animation.Instance.CurrentFrame = GetFrameIndexFromPosition(p.X);
        var trackView = itemsControl_trackViews.GetVisualDescendants().OfType<AnimationModificationTrackView>().FirstOrDefault(tv => {
            Rect bounds = tv.Bounds;
            Point p_local = e.GetPosition(tv);
            return bounds.Contains(p_local);
        });
        if (trackView != null) {
            AnimationEditor.Instance.SelectedModificationTrack = trackView.ViewModel;
        }
        isPointerPressed = true;
    }
    private void Canvas_FrameIndicator_PointerMoved(object? sender, PointerEventArgs e) {
        if (isPointerPressed) {
            Animation.Instance.CurrentFrame = GetFrameIndexFromPosition(e.GetPosition(canvas_frameIndicator).X);
        }
    }
    private void Canvas_FrameIndicator_PointerReleased(object? sender, PointerReleasedEventArgs e) { 
        isPointerPressed = false;
    }
    private void Canvas_FrameIndicator_SizeChanged(object? sender, SizeChangedEventArgs e) {
        currentFrameIndicator.Height = canvas_frameIndicator.Bounds.Height;
        currentFrameIndicator[Canvas.LeftProperty] = GetLeftForFrameIndex(Animation.Instance.CurrentFrame);
    }
}
