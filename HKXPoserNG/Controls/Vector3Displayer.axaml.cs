using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Numerics;
using DependencyPropertyGenerator;

namespace HKXPoserNG;

[DependencyProperty("Vector", typeof(Vector3))]
public partial class Vector3Displayer : UserControl {
    public Vector3Displayer() {
        InitializeComponent();
    }

    partial void OnVectorChanged(Vector3 newValue) {
        textBlockX.Text = newValue.X.ToString();
        textBlockY.Text = newValue.Y.ToString();
        textBlockZ.Text = newValue.Z.ToString();
    }

}