using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HKXPoserNG.Extensions;
using Material.Icons.Avalonia;
using System;
using System.Diagnostics;

namespace HKXPoserNG.Views;

public partial class AnimationController : UserControl {
    public AnimationController() {
        DataContext = ViewModels.Animation.Instance;
        InitializeComponent();
        MaterialIcon icon = new() {
            Kind = Material.Icons.MaterialIconKind.Play
        };
        button_playPause.Content = icon;
        ViewModels.Animation.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ViewModels.Animation.IsPlaying)) {
                if (ViewModels.Animation.Instance.IsPlaying) {
                    icon.Kind = Material.Icons.MaterialIconKind.Pause;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    float startTime = ViewModels.Animation.Instance.CurrentPose.Time;
                    void DispatcherRecursion() {
                        Dispatcher.UIThread.WaitForConditionAndContinue(
                            () => {
                                int nextFrame = ViewModels.Animation.Instance.CurrentFrame + 1;
                                if (nextFrame >= ViewModels.Animation.Instance.FrameCount) {
                                    ViewModels.Animation.Instance.IsPlaying = false;
                                    return true;
                                }
                                float nextTime = ViewModels.Animation.Instance.Poses[ViewModels.Animation.Instance.CurrentFrame + 1].Time;
                                return startTime + stopwatch.Elapsed.TotalSeconds > nextTime;
                            },
                            () => {
                                if (ViewModels.Animation.Instance.IsPlaying) {
                                    ViewModels.Animation.Instance.CurrentFrame++;
                                    DispatcherRecursion();
                                }
                            }
                        );
                    }
                    DispatcherRecursion();
                } else {
                    icon.Kind = Material.Icons.MaterialIconKind.Play;
                }
            }
        };
    }
}