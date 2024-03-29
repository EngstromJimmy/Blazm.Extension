﻿using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text.RegularExpressions;
using BlazmExtension.Dialogs.ComponentReferences;
using System.IO;
using System.Text;
using BlazmExtension.Extensions;

namespace BlazmExtension
{
    [Command(PackageIds.FindComponentUsages)]
    internal sealed class FindComponentUsagesCommand : BaseCommand<FindComponentUsagesCommand>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        public static FindComponentUsagesCommand Instance { get; private set; }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

            // Get the current document and caret position
            var activeDocument = dte.ActiveDocument;
            var textSelection = (TextSelection)activeDocument.Selection;
            var point = textSelection.ActivePoint;
            var lineText = point.CreateEditPoint().GetLines(point.Line, point.Line + 1);
            int cursorPos = point.DisplayColumn - 1; // Convert to 0-based indexing
            var componentName = lineText.GetComponentNameOnCursor(cursorPos);


            if (string.IsNullOrEmpty(componentName))
            {
                try
                {
                    componentName = Path.GetFileNameWithoutExtension(dte.ActiveDocument.FullName);
                }
                catch { }
            }

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