using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;

// NuGet console: Install-Package WixSharp
// NuGet Manager UI: browse tab

namespace WixSharp_Setup
{
    class Program
    {
        static void Main()
        {
            var selectedExe = "MifareWindowsTool.exe";
            var solDir = System.IO.Directory.GetParent(System.IO.Directory.GetParent(System.IO.Directory.GetParent(System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName).FullName).FullName).FullName + "\\MifareWindowsTool";
            var mode = "Release";
#if DEBUG
            mode = "Debug";
#endif

            var buildDir = System.IO.Path.Combine(solDir, $"bin\\{mode}");
            var exepath = System.IO.Path.Combine(buildDir, selectedExe);
            Feature binaries = new Feature("MWT Binaries");
            var project = new Project("MifareWindowsTool",
                              new Dir(@"%ProgramFiles%\AVXTEC\MWT"
                                , new Files(buildDir + @"\*.*")
                                 , new WixSharp.File(new Id("MWT_exe"), binaries, exepath)),
                              new LaunchApplicationFromExitDialog(exeId: "MWT_exe", description: "Launch Mifare Windows Tool (MWT)")
                              , new InstalledFileAction("MWT_exe", "")
                              );
            project.SetNetFxPrerequisite("WIX_IS_NETFRAMEWORK_462_OR_LATER_INSTALLED >= '#528040'", "requires .NET Framework 4.8 or higher.");
            project.ProductId = Guid.NewGuid();

            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25779b");
            var strAssemblyFileVersion = AssemblyName.GetAssemblyName(exepath).Version;
            project.Version = strAssemblyFileVersion;
            var pName = $"MWT v{strAssemblyFileVersion}";
            project.Name = pName;
            project.MajorUpgrade = new MajorUpgrade()
            {
                AllowDowngrades = true,
                Schedule = UpgradeSchedule.afterInstallInitialize,
            };
            project.UI = WUI.WixUI_InstallDir;
            project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.InstallDirDlg);

            var IconFilename = System.IO.Path.Combine(solDir, "MWT.ico");
            var desktopShortcut = new FileShortcut(selectedExe, "%Desktop%")
            {
                Name = pName
            };
            var programMenuShortCut = new FileShortcut(selectedExe, @"%ProgramMenu%")
            {
                Name = pName
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
            var outputpath = project.BuildMsi();
            if (outputpath != null)
                Process.Start(Path.GetDirectoryName(outputpath));

        }
    }
}