using System.Windows;
using System.Windows.Controls;

namespace FileDB.View
{
    // Source: https://stackoverflow.com/questions/1000040/data-binding-to-selecteditem-in-a-wpf-treeview
    public class ExtendedTreeView : TreeView
    {
        public ExtendedTreeView() : base()
        {
            SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(___ICH);
        }

        void ___ICH(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                SetValue(SelectedItem_Property, SelectedItem);
            }
        }

        public object SelectedItem_
        {
            get { return (object)GetValue(SelectedItem_Property); }
            set { SetValue(SelectedItem_Property, value); }
        }
        public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("SelectedItem_", typeof(object), typeof(ExtendedTreeView), new UIPropertyMetadata(null));
    }
}
