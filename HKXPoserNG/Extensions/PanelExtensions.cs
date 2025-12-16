using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace HKXPoserNG.Extensions;

public static class PanelExtensions {
    public static TPanel AddChildren<TPanel>(this TPanel panel, IEnumerable<Control> children) where TPanel : Panel {
        foreach (Control child in children) {
            panel.Children.Add(child);
        }
        return panel;
    }
}
