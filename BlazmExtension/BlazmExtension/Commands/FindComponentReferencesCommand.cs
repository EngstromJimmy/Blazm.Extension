using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text.RegularExpressions;
using BlazmExtension.Dialogs.ComponentReferences;


namespace BlazmExtension
{
    [Command(PackageIds.FindComponentReferences)]
    internal sealed class FindComponentReferencesCommand : BaseCommand<FindComponentReferencesCommand>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        public static FindComponentReferencesCommand Instance { get; private set; }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Get the current document and caret position
            var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));


            var activeDocument = dte.ActiveDocument;
            var textSelection = (TextSelection)activeDocument.Selection;
            var point = textSelection.ActivePoint;
            var lineText = point.CreateEditPoint().GetLines(point.Line, point.Line + 1);

            // Use regex to get the component name
            var componentNameRegex = new Regex(@"<(\w+)(?:\s*[^>]*?/?>|>)");
            var match = componentNameRegex.Match(lineText); // Searching backwards from the caret
            var componentName = match.Groups[1].Value;

            if (string.IsNullOrWhiteSpace(componentName))
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProvider.GlobalProvider,
                    "Unable to determine the component name.",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
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
                control.Initialize(componentName);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {

            }
        }
    }
}