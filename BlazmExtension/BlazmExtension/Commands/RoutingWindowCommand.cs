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

namespace BlazmExtension;

[Command(PackageIds.RoutingWindow)]
internal sealed class RoutingWindowCommand : BaseCommand<RoutingWindowCommand>
{
    protected override Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        return base.InitializeCompletedAsync();
    }
    public static RoutingWindowCommand Instance
    {
        get;
        private set;
    }
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        try
        {
            ToolWindowPane window = Package.FindToolWindow(typeof(RoutingWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

        }
        catch (Exception ex )
        {

            throw;
        }
        

    }
}
