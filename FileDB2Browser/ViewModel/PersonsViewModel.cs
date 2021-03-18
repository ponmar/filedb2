using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
{
    public class PersonsViewModel
    {
        private readonly FileDB2Handle fileDB2Handle;

        public PersonsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
        }
    }
}
