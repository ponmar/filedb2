using System.Windows;
using FileDB.ViewModel;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for AddPersonWindow.xaml
    /// </summary>
    public partial class AddPersonWindow : Window
    {
        public AddPersonWindow(int? personId = null)
        {
            InitializeComponent();
            DataContext = new AddPersonViewModel(personId);
        }
    }
}
