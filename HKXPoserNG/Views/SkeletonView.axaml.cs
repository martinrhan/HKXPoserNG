using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Layout;
using Material.Icons;
using Material.Icons.Avalonia;
using HKXPoserNG.Extensions;
using HKXPoserNG.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System;
using HKXPoserNG.Reactive;
using System.Reactive.Linq;
using HKXPoserNG.Mvvm;
using System.Diagnostics;
using Avalonia.Input;

namespace HKXPoserNG.Views;

public partial class SkeletonView : UserControl {
    public SkeletonView() {
        this.DataContext = Skeleton.Instance;
        InitializeComponent();
        treeView.SelectionChanged += TreeView_SelectionChanged;

        var observable_selectedBone = Skeleton.Instance.GetPropertyValueObservable(nameof(Skeleton.SelectedBone), s => s.SelectedBone);
        var observable_selectedBoenAffectedBySelectedModificationTrack = observable_selectedBone
            .Select(bone => bone?.GetPropertyValueObservable(nameof(Bone.IsAffectedBySelectedModificationTrack), b => b.IsAffectedBySelectedModificationTrack) ?? Observable.Return(false))
            .Switch();

        treeView.ItemTemplate = new FuncTreeDataTemplate<Bone>(
            (bone, _) => new StackPanel() { Orientation = Orientation.Horizontal }.
                AddChildren([
                    new TextBlock(){
                        [ForegroundProperty.Bind()] = bone.GetPropertyValueObservable(
                            nameof(Bone.IsAffectedBySelectedModificationTrack), b => b.IsAffectedBySelectedModificationTrack ? Brushes.Red : Brushes.Black).
                            ToBinding(),
                        Text = $"{bone.Index}  {bone.Name}"
                    },
                    new Button(){
                        Padding = new(0),
                        Margin = new(5,0,0,0),
                        [IsVisibleProperty.Bind()] = observable_selectedBone.Select(
                            selectedBone => {
                                if (selectedBone != Skeleton.Instance.SelectedBone) throw new Exception();
                                Debug.WriteLine($"selectedBone: {selectedBone}, bone: {bone}");
                                return selectedBone == bone;
                            }).
                            ToBinding(),
                        [IsEnabledProperty.Bind()] = AnimationEditor.Instance.GetPropertyValueObservable(
                            nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack != null).
                            ToBinding(),
                        [ContentProperty.Bind()] = observable_selectedBoenAffectedBySelectedModificationTrack.
                            Select(b => new MaterialIcon(){Kind = b ? MaterialIconKind.Minus : MaterialIconKind.Plus}).
                            ToBinding(),
                        Command = new SimpleCommand(() => {
                            var selectedModificationTrack = AnimationEditor.Instance.SelectedModificationTrack;
                            if (selectedModificationTrack == null) throw new Exception();
                            if (bone.IsAffectedBySelectedModificationTrack) {
                                selectedModificationTrack.RemoveAffectedBone(bone);
                            } else {
                                selectedModificationTrack.AddAffectedBone(bone);
                            }
                        }),
                    }
                ])
            ,
            bone => bone.Children
        );
    }

    private void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        Debug.WriteLine("TreeView_SelectionChanged, Bones: " + string.Join(", ", e.AddedItems.OfType<Bone>().Select(b => b.ToString())));
        foreach (Bone bone in e.AddedItems.OfType<Bone>()) {
            Stack<Bone> path = new();
            Bone? b = bone;
            do {
                path.Push(b);
                b = b.Parent;
            } while (b != null);

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        MainWindow mainWindow = (this.VisualRoot as MainWindow)!;
        mainWindow.KeyDown += (_, e) => {
            Bone? selectedBone = Skeleton.Instance.SelectedBone;
            if (selectedBone == null) return;
            AnimationModificationTrack? selectedModificationTrack = AnimationEditor.Instance.SelectedModificationTrack;
            if (selectedModificationTrack == null) return;
            if (selectedBone.IsAffectedBySelectedModificationTrack) {
                if (e.Key == Key.OemMinus) selectedModificationTrack!.RemoveAffectedBone(selectedBone);
            } else {
                if (e.Key == Key.OemPlus) selectedModificationTrack!.AddAffectedBone(selectedBone);
            }
        };
    }
}