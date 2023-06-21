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

namespace BlazmExtension;

[Command(PackageIds.ExtractToComponent)]
internal sealed class ExtractToComponentCommand : BaseCommand<ExtractToComponentCommand>
{
    protected override Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        return base.InitializeCompletedAsync();
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;

        var document = dte.ActiveDocument;
        var selection = (TextSelection)dte.ActiveDocument.Selection;
        var importsPath = Path.GetDirectoryName(document.FullName);

        //Get name for component
        string componentname = "";
        System.Windows.Window window = (System.Windows.Window)HwndSource.FromHwnd(dte.MainWindow.HWnd).RootVisual;
        FileNameDialog dialog = new FileNameDialog()
        {

            Owner = window
        };

        bool? result = dialog.ShowDialog();
        if (result.HasValue && result.Value)
        {
            componentname = (result.HasValue && result.Value) ? dialog.Input : string.Empty;

            if (componentname == string.Empty)
            {
                componentname = "Component";
            }
            componentname = Path.GetFileNameWithoutExtension(componentname);
            componentname = char.ToUpper(componentname[0]) + componentname.Substring(1); // capitalize the first letter

            var newcomponentPath = importsPath + "\\" + componentname + ".razor";
            if (!File.Exists(newcomponentPath))
            {
                File.WriteAllText(newcomponentPath, selection.Text);
            }

            selection.Text = $"<{componentname} />";

            dte.ItemOperations.OpenFile(newcomponentPath);
        }
    }
}
