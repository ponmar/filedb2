using FileDBApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBApp.ViewModel
{
    [QueryProperty("PersonModel", "PersonModel")]
    public class PersonDetailsViewModel : ViewModelBase
    {
        public PersonModel PersonModel
        {
            get => personModel;
            set
            {
                personModel = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        private PersonModel personModel;

        public string Title => PersonModel != null ? $"{PersonModel.Firstname} {PersonModel.Lastname}" : String.Empty;

        public PersonDetailsViewModel()
        {
        }
    }
}
