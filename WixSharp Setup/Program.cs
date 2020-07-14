using System;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Forms;

// NuGet console: Install-Package WixSharp
// NuGet Manager UI: browse tab

namespace WixSharp_Setup
{
    class Program
    {
        static void Main()
        {
            var selectedExe = "MifareWindowsTool.exe";
            var solDir = System.IO.Directory.GetParent(System.IO.Directory.GetParent(System.IO.Directory.GetParent(System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName).FullName).FullName).FullName+ "\\MifareWindowsTool";
            var buildDir = System.IO.Path.Combine(solDir, "bin\\Release");
            var project = new ManagedProject("MifareWindowsTool",
                              new Dir(@"%ProgramFiles%\AVXTEC\MWT"
                                , new Files(buildDir + @"\*.*")));
            project.SetNetFxPrerequisite("NETFRAMEWORK45 >= '#528033'", "Please install .Net Framework 4.8 First");
            project.ProductId = Guid.NewGuid();
            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25779b");
            project.ManagedUI = new ManagedUI();
            project.Name = "MWT";
            project.ManagedUI.InstallDialogs
                                .Add(Dialogs.InstallDir)
                                .Add(Dialogs.Progress)
                                .Add(Dialogs.Exit);

            project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
                                           .Add(Dialogs.Progress)
                                           .Add(Dialogs.Exit);
            var IconFilename = System.IO.Path.Combine(solDir, "MWT.ico");
           
            var desktopShortcut = new FileShortcut(selectedExe, "%Desktop%")
            {
                Name = "MWT"
            };
            var programMenuShortCut = new FileShortcut(selectedExe, @"%ProgramMenu%")
            {
                Name = $"MWT"
            };
            if (!string.IsNullOrWhiteSpace(IconFilename))
            {
                desktopShortcut.IconFile = IconFilename;
                programMenuShortCut.IconFile = IconFilename;
            }
            project.ResolveWildCards(true)
                .FindFirstFile(selectedExe)
                .Shortcuts = new[] {
                                       desktopShortcut,
                                       programMenuShortCut
                              };
            
            project.ControlPanelInfo.UrlInfoAbout = "https://github.com/xavave/Mifare-Windows-Tool";
            project.ControlPanelInfo.ProductIcon = IconFilename;
            project.ControlPanelInfo.Contact = "AVXTEC";
            project.ControlPanelInfo.Manufacturer = "AVXTEC";
            project.BuildMsi();
        }
    }
}