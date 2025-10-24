using BrokenLinkChecker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BrokenLinkChecker.ViewModels
{
    public partial class SettingsVM : BaseVM
    {

        private readonly SettingsWnd window;


        [ObservableProperty]
        public int maxThreads;

        [ObservableProperty]
        public string userAgent;

        [ObservableProperty]
        public int delayMin;

        [ObservableProperty]
        public int delayMax;

        public SettingsVM(SettingsWnd window)
        {
            this.window = window;
            MaxThreads = appsettings.Default.MaxThreads;
            UserAgent = appsettings.Default.UserAgent;
            DelayMax = appsettings.Default.MaxDelay;
            DelayMin = appsettings.Default.MinDelay;
        }

        [RelayCommand]
        public void Accept()
        {

            appsettings.Default.MaxThreads = MaxThreads;
            appsettings.Default.UserAgent = UserAgent;
            appsettings.Default.MaxDelay = DelayMax;
            appsettings.Default.MinDelay = DelayMin;
            appsettings.Default.Save();
            window.Close();
        }

        [RelayCommand]
        public void Cancel()
        {
            window.Close();

        }
    }
}
