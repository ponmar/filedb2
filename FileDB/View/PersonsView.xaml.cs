using System.Windows.Controls;
using System.Windows.Input;
using FileDB.ViewModel;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for Persons.xaml
    /// </summary>
    public partial class PersonsView : UserControl
    {
        public PersonsView()
        {
            InitializeComponent();
            var model = Model.Model.Instance;
            DataContext = new PersonsViewModel(model.Config, model.DbAccess, ServiceLocator.Resolve<IDialogs>());
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Handle event here to avoid no scrolling over DataGrid within this ScrollViewer
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
