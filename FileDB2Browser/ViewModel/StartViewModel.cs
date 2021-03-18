using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
{
    public class StartViewModel
    {
        public int NumFiles { get; } = 100;

        private readonly FileDB2Handle fileDB2Handle;

        public StartViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
        }
    }
}
