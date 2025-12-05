using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DependencyPropertyGenerator;
using System;
using System.Collections.Specialized;
using System.Linq;
using HKXPoserNG.Reactive;
using HKXPoserNG.ViewModels;
using Transform = HKXPoserNG.ViewModels.Transform;
using System.Runtime.CompilerServices;

namespace HKXPoserNG.Views;

[DependencyProperty("ViewModel", typeof(AnimationModificationTrack))]
public partial class AnimationModificationTrackView : Canvas {
    public AnimationModificationTrackView() {
        IsHitTestVisible = true;
        Focusable = true;
        Background = Brushes.White;
    }

    private Action? unsubscribe;
    partial void OnViewModelChanged(AnimationModificationTrack? oldValue, AnimationModificationTrack? newValue) {
        unsubscribe?.Invoke();
        if (ViewModel == null) return;
        var subscription0 = ViewModel.
            GetPropertyObservable(nameof(AnimationModificationTrack.IsSelected), amt => amt.IsSelected).
            Subscribe(b => this.Background = b ? Brushes.LightBlue : Brushes.White);
        var subscription1 = ViewModel.
            KeyFrames.GetCollectionChangedObservable().
            Subscribe(ep => KeyFrames_CollectionChanged(ep.EventArgs));
        var subscription2 = ViewModel.
             KeyFrameIntervals.GetItemsObservable(
                 interval => interval.GetPropertyObservable(nameof(IKeyFrameInterval.InterpolationFunction), kf => kf.InterpolationFunction)
             ).
             Subscribe(t => KeyFrameIntervals_ItemInterpolationFunctionChanged(t.Item1, t.Item2));
        unsubscribe = () => {
            subscription0.Dispose();
            subscription1.Dispose();
            subscription2.Dispose();
        };
    }

    private void SetKeyFrameIndicatorShape(Panel indicator, IKeyFrame keyFrame) {
        indicator.Height = this.Bounds.Height;
        indicator[Canvas.LeftProperty] = GetLeftFromFrame(keyFrame.Frame);
    }
    private void KeyFrames_CollectionChanged(NotifyCollectionChangedEventArgs e) {
        void AddFrameIndicator(IKeyFrame keyFrame) {
            Panel indicator = new() {
                DataContext = keyFrame,
                Background = Brushes.Black,
                Width = 1,
            };
            SetKeyFrameIndicatorShape(indicator, keyFrame);
            Children.Add(indicator);
        }
        void RemoveFrameIndicator(IKeyFrame keyFrame) {
            var toRemove = Children.First(c => c.DataContext == keyFrame);
            Children.Remove(toRemove);
        }
        if (e.OldItems != null) foreach (IKeyFrame oldItem in e.OldItems) RemoveFrameIndicator(oldItem);
        if (e.NewItems != null) foreach (IKeyFrame newItem in e.NewItems) AddFrameIndicator(newItem);
    }

    private void SetKeyFrameIntervalIndicatorShape(Panel indicator, IKeyFrameInterval interval) {
        (IKeyFrame kf0, IKeyFrame kf1) = ViewModel!.GetKeyFramesBesideInterval(interval);
        indicator.Width = GetLeftFromFrame(kf1.Frame - kf0.Frame);
        indicator[Canvas.LeftProperty] = GetLeftFromFrame(kf0.Frame);
        indicator[Canvas.TopProperty] = this.Bounds.Height / 2;
    }
    private void KeyFrameIntervals_ItemInterpolationFunctionChanged(IKeyFrameInterval interval, Func<Transform, Transform, float, Transform>? newInterpolationFunction) {
        void AddIntervalIndicator(IKeyFrameInterval interval) {
            Panel indicator = new() {
                DataContext = interval,
                Background = Brushes.Black,
                Height = 1,
            };
            SetKeyFrameIntervalIndicatorShape(indicator, interval);
            Children.Add(indicator);
        }
        void RemoveFrameIndicator(IKeyFrameInterval interval) {
            Control? toRemove = Children.FirstOrDefault(c => c.DataContext == interval);
            if (toRemove != null) Children.Remove(toRemove);
        }
        if (newInterpolationFunction == null) RemoveFrameIndicator(interval);
        else AddIntervalIndicator(interval);
    }

    private int MaxFrameIndex => Animation.Instance.FrameCount - 1;
    public double GetLeftFromFrame(int frame) {
        return frame / (double)MaxFrameIndex * this.Bounds.Width;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        unsubscribe?.Invoke();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        foreach (var child in Children) {
            switch (child.DataContext) {
                case IKeyFrame keyFrame:
                    SetKeyFrameIndicatorShape((Panel)child, keyFrame);
                    break;
                case IKeyFrameInterval interval:
                    SetKeyFrameIntervalIndicatorShape((Panel)child, interval);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}