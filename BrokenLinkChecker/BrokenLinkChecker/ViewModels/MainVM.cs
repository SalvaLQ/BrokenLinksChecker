using Avalonia.Controls;
using BrokenLinkChecker.Domain;
using BrokenLinkChecker.Infraestructure.Browser;
using BrokenLinkChecker.Infraestructure.Files;
using BrokenLinkChecker.LinkChecker.Core;
using BrokenLinkChecker.LinkChecker.Core.Enums;
using BrokenLinkChecker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BrokenLinkChecker.LocalizationResources.MainRes;
namespace BrokenLinkChecker.ViewModels;

public partial class MainVM : BaseVM
{

    private CancellationTokenSource tokenSource;

    private readonly MainWnd window;

    [ObservableProperty]
    public string pageSitesSearch;

    [ObservableProperty]
    public int totalSites;

    public List<PageSite> Sites { get; set; }

    [ObservableProperty]
    public ObservableCollection<Domain.PageLink> verifiedLinks;
    

    [ObservableProperty]
    public bool canStart;

    [ObservableProperty]
    public bool canStop;

    [ObservableProperty]
    public int totalBrokenLinks;

    [ObservableProperty]
    public int totalLinks;
 

   
    public MainVM(MainWnd window)
    {
        this.window = window;
        CanStart = true;
        CanStop = false;
        PageSitesSearch = "";
        VerifiedLinks = new ObservableCollection<PageLink>();


    }
     
 

    [RelayCommand]
    public async void Start()
    {
        CanStart = false;
        CanStop = true;
        await Launch();


    }
    [RelayCommand]
    public void Stop()
    {
        tokenSource.Cancel();
        CanStart = true;
        CanStop = false;
    }



    private async Task Launch()
    {
        tokenSource = new CancellationTokenSource();

        bool failed = false;
        Random rnd = new Random();
        Sites = new List<PageSite>();
        if (PageSitesSearch.Contains(","))
            PageSitesSearch.Split(",").ToList<string>().ForEach(d => Sites.Add(new PageSite() { SiteUrl = d }));
        else
            Sites.Add(new PageSite() { SiteUrl = PageSitesSearch });
        TotalSites = Sites.Count();




        var progress = new Progress<PageLink>(info =>
        {

            TotalLinks++;
            if (info.Broken)
            {
                VerifiedLinks.Add(info);
                TotalBrokenLinks++;
            }
        });
        LinkVerificationOption opt = new LinkVerificationOption();        
        string UserAgent;
        LinkCheckerEngine linkChecker = new LinkChecker.Core.LinkCheckerEngine(Sites,appsettings.Default.MaxThreads,  appsettings.Default.UserAgent,Utils.Purchased,appsettings.Default.MaxDelay,appsettings.Default.MinDelay);
        var res = await linkChecker.CheckLinks(progress, tokenSource.Token);
      
        var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
        {
            ContentHeader = CompletedTitle,
            SupportingText = CompletedMsg,
            StartupLocation = WindowStartupLocation.CenterOwner,
            Borderless = true,
            Width = 400,
            DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Info,
        });
        var result = await dialog.ShowDialog(window);



        CanStart = true;
        CanStop = false;
        if (failed)
        {
            var er = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                ContentHeader = LocalizationResources.CommonRes.ErrorTitle,
                SupportingText = ErrorInfo,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Borderless = true,
                DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Error,
            });
            var resEr = await er.ShowDialog(window);
        }
    }

    
    [RelayCommand]
    public async void ExportCsv()
    {
        if (VerifiedLinks == null || !VerifiedLinks.Any())
            return;
        var dlg = new SaveFileDialog();
        string FileExt;

        dlg.DefaultExtension = CsvExt;
        dlg.Filters.Add(new FileDialogFilter() { Name = CsvFileName, Extensions = { CsvExt } });
        FileExt = "." + CsvExt;

        dlg.InitialFileName = Infraestructure.Files.FilesUtils.MakeValidFileName("Links " + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortDateString() + FileExt);
        var result = await dlg.ShowAsync(window);
        if (result != null)
        {
            var destinationPath = result;
            Infraestructure.Files.FileExporter FileExp = new Infraestructure.Files.FileExporter(destinationPath);

            FileExp.ExpExportFileCSV(VerifiedLinks.ToList());
            var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                ContentHeader = ExportTitle,
                SupportingText = ExportedOk,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Borderless = true,
                Width = 400,
                DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Info,
            });
            _ = await dialog.ShowDialog(window);
        }
    }

    [RelayCommand]
    public async void ExportExcel()
    {
        if (VerifiedLinks == null || !VerifiedLinks.Any())
            return;
        var dlg = new SaveFileDialog();
        string FileExt;

        dlg.DefaultExtension = ExcelExt;
        dlg.Filters.Add(new FileDialogFilter() { Name = ExcelFileName, Extensions = { ExcelExt } });
        FileExt = "." + ExcelExt;

        dlg.InitialFileName = FilesUtils.MakeValidFileName("Links " + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortDateString() + FileExt);
        var result = await dlg.ShowAsync(window);
        if (result != null)
        {
            var destinationPath = result;
            Infraestructure.Files.FileExporter FileExp = new Infraestructure.Files.FileExporter(destinationPath);
            List<string> Fields = new List<string>() { HeadUrl, HeadLinkText, HeadPage };
            FileExp.ExpExportFileExcel(VerifiedLinks.ToList(), Fields);
            var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                ContentHeader = ExportTitle,
                SupportingText = ExportedOk,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Borderless = true,
                Width = 400,
                DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Info,
            });
            _ = await dialog.ShowDialog(window);
        }
    }
    [RelayCommand]
    public void Settings()
    {
        SettingsWnd sett = new SettingsWnd();
        SettingsVM settVm = new SettingsVM(sett);
        sett.DataContext = settVm;
        sett.Show(window);
    }

     
    [RelayCommand]
    public void About()
    {
        AboutWnd about = new AboutWnd();
        about.Show(window);

    }

    [RelayCommand]
    public void ClearResults()
    {
        if (VerifiedLinks != null)
        {
            VerifiedLinks.Clear();
            TotalBrokenLinks = 0;
            TotalSites = 0;
            totalLinks = 0;
        }
    }
    [RelayCommand]
    public void Exit()
    {
        window.Close();
    }











}
