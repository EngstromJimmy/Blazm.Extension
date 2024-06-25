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

[Command(PackageIds.QuickSave)]
internal sealed class QuickSaveCommand : BaseCommand<QuickSaveCommand>
{
    protected override Task InitializeCompletedAsync()
    {

        
        return base.InitializeCompletedAsync();
    }
    public static QuickSaveCommand Instance
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
            if (dte.ActiveDocument != null)
            {
                SaveDocument(dte.ActiveDocument);
            }

        }
        catch (Exception ex )
        {

            throw;
        }
        

    }

    private void SaveDocument(Document document)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        string filePath = document.FullName;
        TextDocument textDocument = (TextDocument)document.Object("TextDocument");
        File.WriteAllText(filePath, textDocument.CreateEditPoint().GetText(textDocument.EndPoint));

        document.Saved = true; 
    }
}
