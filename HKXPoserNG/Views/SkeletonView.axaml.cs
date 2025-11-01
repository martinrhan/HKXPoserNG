using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HKXPoserNG.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace HKXPoserNG.Views;

public partial class SkeletonView : UserControl {
    public SkeletonView() {
        this.DataContext = Skeleton.Instance;
        InitializeComponent();
        treeView.SelectionChanged += TreeView_SelectionChanged;
    }

    private void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        foreach (Bone bone in e.AddedItems.OfType<Bone>()) {
            Stack<Bone> path = new();
            Bone? b = bone;
            do {
                path.Push(b);
                b = b.Parent;
            }while (b != null);

            Bone rootBone = path.Pop();
            TreeViewItem rootTreeViewItem = treeView.GetVisualDescendants().OfType<TreeViewItem>().FirstOrDefault(tvi => tvi.DataContext == rootBone)!;

            void Recursion(TreeViewItem treeViewItem, Stack<Bone> path) { 
                if (path.Count == 0) return;
                if (treeViewItem.IsExpanded) {
                    Bone nextBone = path.Pop();
                    TreeViewItem nextTreeViewItem = treeViewItem.GetVisualDescendants().OfType<TreeViewItem>().FirstOrDefault(tvi => tvi.DataContext == nextBone)!;
                    Recursion(nextTreeViewItem, path);
                } else {
                    treeViewItem.IsExpanded = true;
                    Dispatcher.UIThread.Post(() => Recursion(treeViewItem, path));
                }
            }
            Recursion(rootTreeViewItem, path);
        }
    }
}