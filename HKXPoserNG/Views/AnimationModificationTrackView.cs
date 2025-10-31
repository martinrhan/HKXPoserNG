using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using DependencyPropertyGenerator;
using HKXPoserNG.Mvvm;
using HKXPoserNG.ViewModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace HKXPoserNG.Views;

[DependencyProperty("ViewModel", typeof(AnimationModificationTrack))]
public partial class AnimationModificationTrackView : Canvas {
    public AnimationModificationTrackView() {
        IsHitTestVisible = true;
        Focusable = true;
        Background = Brushes.White;
    }

    partial void OnViewModelChanged(AnimationModificationTrack? oldValue, AnimationModificationTrack? newValue) {
        if (oldValue != null)
            ClearSubscriptions(oldValue);
        if (ViewModel == null) return;
        ViewModel.KeyFrames.CollectionChanged += KeyFrame_CollectionChanged;
        KeyFrame_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(AnimationModificationTrack.IsSelected)));
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(AnimationModificationTrack.IsSelected)) {
            if (ViewModel == null) return;
            this.Background = ViewModel.IsSelected ? Brushes.LightBlue : Brushes.White;
        }
    }

    private void KeyFrame_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        void AddFrameIndicator(AnimationModificationKeyFrame frame) {
            var indicator = new Panel {
                DataContext = frame,
                Background = Brushes.Black,
                Width = 1,
                Height = this.Bounds.Height,
                [Canvas.LeftProperty] = GetLeftForFrameIndex(frame.FrameIndex)
            };
            Children.Add(indicator);
        }
        void RemoveFrameIndicator(AnimationModificationKeyFrame frame) {
            var toRemove = Children.FirstOrDefault(c => c.DataContext == frame);
            if (toRemove != null) {
                Children.Remove(toRemove);
            }
        }

        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                foreach (var newItem in e.NewItems!)
                    AddFrameIndicator((AnimationModificationKeyFrame)newItem);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var oldItem in e.OldItems!)
                    RemoveFrameIndicator((AnimationModificationKeyFrame)oldItem);
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (var newItem in e.NewItems!)
                    AddFrameIndicator((AnimationModificationKeyFrame)newItem);
                foreach (var oldItem in e.OldItems!)
                    RemoveFrameIndicator((AnimationModificationKeyFrame)oldItem);
                break;
            case NotifyCollectionChangedAction.Reset:
                Children.Clear();
                foreach (var frame in ViewModel!.KeyFrames)
                    AddFrameIndicator(frame);
                break;
        }
    }

    private int MaxFrameIndex => Animation.Instance.FrameCount - 1;
    public double GetLeftForFrameIndex(int frameIndex) {
        return frameIndex / (double)MaxFrameIndex * this.Bounds.Width;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        if (ViewModel != null)
            ClearSubscriptions(ViewModel);
    }

    private void ClearSubscriptions(AnimationModificationTrack oldValue) {
        oldValue.KeyFrames.CollectionChanged -= KeyFrame_CollectionChanged;
        oldValue.PropertyChanged -= ViewModel_PropertyChanged;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        foreach (var child in Children) {
            child[Canvas.LeftProperty] = GetLeftForFrameIndex(((AnimationModificationKeyFrame)child.DataContext!).FrameIndex);
            child.Height = this.Bounds.Height;
        }
    }
}