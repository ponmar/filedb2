﻿using System.Windows.Controls;
using System.Windows.Input;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for TagsView.xaml
/// </summary>
public partial class TagsView : UserControl
{
    public TagsView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<TagsViewModel>();
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Handle event here to avoid no scrolling over DataGrid within this ScrollViewer
        ScrollViewer scv = (ScrollViewer)sender;
        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}
