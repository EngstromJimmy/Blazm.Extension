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
using System.Threading.Tasks;

namespace BlazmExtension
{
    [Command(PackageIds.SwitchToNestedFile)]
    internal sealed class SwitchToNestedFileCommand : BaseCommand<SwitchToNestedFileCommand>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE2 dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
                var document = dte.ActiveDocument;
                List<string> files = new();
                ProjectItem toplevel = null;
                if (document.ProjectItem.ProjectItems.Count == 0)
                {
                    //There are no nested files, try an get the parent file
                    toplevel = document.ProjectItem.Collection.Parent as ProjectItem;
                }
                else
                {
                    toplevel = document.ProjectItem;
                }

                if (toplevel != null && toplevel.Document!=null)
                {
                    files.Add(toplevel.Document.FullName);
                    foreach (ProjectItem item in toplevel.Document.ProjectItem.ProjectItems)
                    {
                        files.Add(item.FileNames[1]);
                    }
                }

                //If files contains a scss file, skip the css and map files
                if (files.Any(f => f.EndsWith(".scss")))
                {
                    files = files.Where(f => !f.EndsWith(".css") && !f.EndsWith(".css.map")).ToList();
                }

                if (files.Count > 1)
                {
                    var index = files.IndexOf(document.FullName);
                    if (index < files.Count - 1)
                    {
                        dte.ItemOperations.OpenFile(files[index + 1]);
                    }
                    else
                    {
                        dte.ItemOperations.OpenFile(files[0]);
                    }
                }
            }
            catch (System.Exception ex)
            {
                
            }
        }
    }
}
