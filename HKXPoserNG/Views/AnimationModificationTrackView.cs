using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using DependencyPropertyGenerator;
using HKXPoserNG.Mvvm;
using HKXPoserNG.ViewModels;
using System;
using System.Collections.Generic;
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
        ViewModel.KeyFrames.CollectionChanged += KeyFrames_CollectionChanged;
        KeyFrames_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(AnimationModificationTrack.IsSelected)));
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(AnimationModificationTrack.IsSelected)) {
            if (ViewModel == null) return;
            this.Background = ViewModel.IsSelected ? Brushes.LightBlue : Brushes.White;
        }
    }

    private void KeyFrames_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        void AddFrameIndicator(int frameIndex) {
            var indicator = new Panel {
                DataContext = frameIndex,
                Background = Brushes.Black,
                Width = 1,
                Height = this.Bounds.Height,
                [Canvas.LeftProperty] = GetLeftForFrameIndex(frameIndex)
            };
            Children.Add(indicator);
        }
        void RemoveFrameIndicator(int frameIndex) {
            var toRemove = Children.First(c => (int)c.DataContext! == frameIndex);
            Children.Remove(toRemove);
        }

        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                foreach (var newItem in e.NewItems!)
                    AddFrameIndicator((int)newItem);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var oldItem in e.OldItems!)
                    RemoveFrameIndicator((int)oldItem);
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (var newItem in e.NewItems!)
                    AddFrameIndicator((int)newItem);
                foreach (var oldItem in e.OldItems!)
                    RemoveFrameIndicator((int)oldItem);
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
        oldValue.KeyFrames.CollectionChanged -= KeyFrames_CollectionChanged;
        oldValue.PropertyChanged -= ViewModel_PropertyChanged;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        foreach (var child in Children) {
            child[Canvas.LeftProperty] = GetLeftForFrameIndex((int)child.DataContext!);
            child.Height = this.Bounds.Height;
        }
    }
}