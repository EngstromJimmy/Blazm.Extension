using EnvDTE;
using EnvDTE80;
using System.IO;

namespace BlazmExtension
{
    [Command(PackageIds.CreateIsolatedCss)]
    internal sealed class CreateIsolatedCssCommand : BaseCommand<CreateIsolatedCssCommand>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            UIHierarchy uih = (UIHierarchy)dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
            Array selectedItems = (Array)uih.SelectedItems;
            foreach (UIHierarchyItem selItem in selectedItems)
            {
                ProjectItem prjItem = selItem.Object as ProjectItem;
                string filePath = prjItem.Properties.Item("FullPath").Value.ToString();

                var newfilePath = filePath + ".css";
                if (!File.Exists(newfilePath))
                {
                    File.WriteAllText(newfilePath, "");
                }
            }
        }
    }
}
