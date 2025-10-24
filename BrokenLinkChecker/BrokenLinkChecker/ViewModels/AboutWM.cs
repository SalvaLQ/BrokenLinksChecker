using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.ViewModels
{
    public partial class AboutWM:BaseVM
    {
        [ObservableProperty]
        public string version;

       
    }
}
