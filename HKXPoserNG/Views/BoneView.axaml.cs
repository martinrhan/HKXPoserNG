using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HKXPoserNG.Views;

public partial class BoneView : UserControl {
    public BoneView() {
        ViewModels.Skeleton.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ViewModels.Skeleton.SelectedBone)) {
                this.DataContext = ViewModels.Skeleton.Instance.SelectedBone;
            }
        };
        this.DataContext = ViewModels.Skeleton.Instance.SelectedBone;
        InitializeComponent();
    }
}