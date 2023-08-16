using EnvDTE;
using System.Collections.Generic;

namespace BlazmExtension.ExtensionMethods
{
    public static class EnvDteExtensionMethods
    {
        public static IEnumerable<string> GetAllRazorFiles(this EnvDTE.Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Projects projects = solution.Projects;
            foreach (EnvDTE.Project project in projects)
            {
                foreach (ProjectItem projectItem in project.ProjectItems)
                {
                    foreach (string file in GetAllRazorFiles(projectItem))
                    {
                        yield return file;
                    }
                }
            }
        }

        public static IEnumerable<string> GetAllRazorFiles(this ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                if (projectItem.Name.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                {
                    yield return projectItem.FileNames[0];
                }
            }
            else if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder ||
                     projectItem.Kind == EnvDTE.Constants.vsProjectItemKindVirtualFolder)
            {
                foreach (ProjectItem subItem in projectItem.ProjectItems)
                {
                    foreach (string file in GetAllRazorFiles(subItem))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
