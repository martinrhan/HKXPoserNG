using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using DependencyPropertyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG.Controls;

[DependencyProperty("Min", typeof(double), DefaultValue = 0.0)]
[DependencyProperty("Max", typeof(double), DefaultValue = 100.0)]
public partial class NumberLine : Canvas {

    partial void OnMinChanged() {
        Redraw();
    }

    partial void OnMaxChanged() {
        Redraw();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        Redraw();
    }

    private void Redraw() {
        Children.Clear();
        double range = Max - Min;
        if (range <= 0) { return; }
        double step = Math.Pow(10, Math.Floor(Math.Log10(range)) - 1);
        for (double v = Math.Ceiling(Min / step) * step; v <= Max; v += step) {
            double x = (v - Min) / range * Bounds.Width;
            var line = new Line {
                StartPoint = new(x, 0),
                EndPoint = new(x, Bounds.Height),
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
            };
            Children.Add(line);
        }
    }
}

