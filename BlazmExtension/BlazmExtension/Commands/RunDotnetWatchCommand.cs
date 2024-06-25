using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using StageCoder;
using System.ComponentModel.Design;
using System.IO.Packaging;
using BlazmExtension.Dialogs.Routing;
using System.Threading;
using System.Diagnostics;

namespace BlazmExtension;

[Command(PackageIds.RunDotnetWatch)]
internal sealed class RunDotnetWatchCommand : BaseCommand<RunDotnetWatchCommand>
{
    protected override Task InitializeCompletedAsync()
    {

        
        return base.InitializeCompletedAsync();
    }
    public static RunDotnetWatchCommand Instance
    {
        get;
        private set;
    }
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        try
        {
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte.SelectedItems.Count == 1)
            {
                var selectedItem = dte.SelectedItems.Item(1);
                var project = selectedItem.Project;
                if (project != null)
                {
                    RunDotnetWatch(project.FullName);
                }
            }

        }
        catch (Exception ex )
        {

            throw;
        }
        

    }

    private void RunDotnetWatch(string projectFilePath)
    {
        try
        {


            // Construct the PowerShell command
            string powershellCommand = $"-NoExit -Command \"Write-Host 'Starting up dotnet watch'; cd {Path.GetDirectoryName(projectFilePath)}; dotnet watch --non-interactive\"";

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments =powershellCommand,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            System.Diagnostics.Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            VsShellUtilities.ShowMessageBox(
                Package,
                $"Error running dotnet watch: {ex.Message}",
                "Run dotnet watch Error",
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
