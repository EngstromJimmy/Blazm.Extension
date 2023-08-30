using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text.RegularExpressions;
using BlazmExtension.Dialogs.ComponentReferences;
using System.IO;
using EnvDTE80;

namespace BlazmExtension
{
    [Command(PackageIds.FindComponentUsagesSE)]
    internal sealed class FindComponentUsagesSECommand : BaseCommand<FindComponentUsagesSECommand>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        public static FindComponentUsagesSECommand Instance { get; private set; }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            UIHierarchy uih = (UIHierarchy)dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
            Array selectedItems = (Array)uih.SelectedItems;

            string fileNameWithoutExtension = null;
            // Ensure only one item is selected
            if (selectedItems.Length == 1)
            {
                UIHierarchyItem selItem = selectedItems.GetValue(0) as UIHierarchyItem;

                if (selItem.Object is ProjectItem prjItem)
                {
                    string filePath = prjItem.Properties.Item("FullPath").Value.ToString();

                    // Check if the file is a Razor file
                    if (Path.GetExtension(filePath).Equals(".razor", StringComparison.OrdinalIgnoreCase))
                    {
                        fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    }
                }
            }
            else if (selectedItems.Length == 0)
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProvider.GlobalProvider,
                    "Error: No files selected.",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProvider.GlobalProvider,
                    "Error: Multiple files selected.",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProvider.GlobalProvider,
                    "Unable to determine the component name.",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
                return;
            }

            try
            {
                ToolWindowPane window = Package.FindToolWindow(typeof(ComponentReferencesWindow), 0, true);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                var control = (ComponentReferencesControl)window.Content;
                control.Initialize(fileNameWithoutExtension);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {

            }
        }
    }
}