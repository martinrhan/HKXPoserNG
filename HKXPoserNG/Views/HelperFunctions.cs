using Avalonia.Controls;
using Avalonia.Layout;
using Material.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Material.Icons;

namespace HKXPoserNG.Views;

public static class HelperFunctions {
    public static void AddCollapsibleControl(this DockPanel panel, Control control, Dock buttonDock) {
        MaterialIconKind GetMaterialIconKind(Dock dock) =>
            dock switch {
                Dock.Left => MaterialIconKind.MenuRight,
                Dock.Top => MaterialIconKind.MenuDown,
                Dock.Right => MaterialIconKind.MenuLeft,
                Dock.Bottom => MaterialIconKind.MenuUp,
                _ => throw new NotImplementedException(),
            };
        Dock GetOppositeDock(Dock dock) =>
            dock switch {
                Dock.Left => Dock.Right,
                Dock.Top => Dock.Bottom,
                Dock.Right => Dock.Left,
                Dock.Bottom => Dock.Top,
                _ => throw new NotImplementedException(),
            };

        MaterialIcon icon = new() {
            Kind = GetMaterialIconKind(buttonDock)
        };
        Button button = new() {
            Content = icon,
            [DockPanel.DockProperty] = buttonDock,
        };
        panel.Children.Add(button);
        panel.Children.Add(control);
        button.Click += (_, _) => {
            if (control.IsVisible) {
                control.IsVisible = false;
                Dock oppositeDock = GetOppositeDock(buttonDock);
                icon.Kind = GetMaterialIconKind(oppositeDock);
                button[DockPanel.DockProperty] = oppositeDock;
            } else {
                control.IsVisible = true;
                icon.Kind = GetMaterialIconKind(buttonDock);
                button[DockPanel.DockProperty] = buttonDock;
            }
        };
    }
}
