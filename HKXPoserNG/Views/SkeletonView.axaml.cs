using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HKXPoserNG.ViewModels;
using System.Reactive;
using System.Reactive.Linq;

namespace HKXPoserNG.Views;

public partial class SkeletonView : UserControl {
    public SkeletonView() {
        this.DataContext = Skeleton.Instance;
        InitializeComponent();
    }
}