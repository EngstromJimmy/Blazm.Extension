using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.Policy;

namespace BlazmExtension
{
    [Command(PackageIds.CreateIsolatedJavaScript)]
    internal sealed class CreateIsolatedJavaScriptCommand : BaseCommand<CreateIsolatedJavaScriptCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            UIHierarchy uih = (UIHierarchy)dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
            Array selectedItems = (Array)uih.SelectedItems;
            foreach (UIHierarchyItem selItem in selectedItems)
            {
                ProjectItem prjItem = selItem.Object as ProjectItem;
                string filePath = prjItem.Properties.Item("FullPath").Value.ToString();

                var newfilePath = filePath + ".js";
                if (!File.Exists(newfilePath))
                {
                    File.WriteAllText(newfilePath, "");
                }
            }
        }
    }
}
