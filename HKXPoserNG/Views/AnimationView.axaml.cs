using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HKXPoserNG.Views;

public partial class AnimationView : UserControl {
    public AnimationView() {
        InitializeComponent();
        DataContext = ViewModels.Animation.Instance;
    }
}