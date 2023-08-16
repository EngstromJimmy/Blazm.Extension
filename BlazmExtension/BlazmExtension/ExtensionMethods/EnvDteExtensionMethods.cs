using EnvDTE;
using System.Collections.Generic;
using System.Diagnostics;
using Project = EnvDTE.Project;

namespace BlazmExtension.ExtensionMethods
{
    public static class EnvDteExtensionMethods
    {
        public static IEnumerable<string> GetAllRazorFiles(this EnvDTE.Solution solution)
        {
            if (solution == null)
            {
                yield break;
            }
            ThreadHelper.ThrowIfNotOnUIThread();
            Projects projects = solution.Projects;
            foreach (Project project in projects)
            {
                if (project.Kind == Constants.vsProjectItemKindSolutionItems)  // Solution folder
                {
                    foreach (ProjectItem subProjectItem in project.ProjectItems)
                    {
                        if (subProjectItem.SubProject != null)  // If this ProjectItem has a sub-project
                        {
                            foreach (string file in GetAllRazorFiles(subProjectItem.SubProject))
                            {
                                yield return file;
                            }
                        }
                        else
                        {
                            foreach (string file in GetAllRazorFiles(subProjectItem))
                            {
                                yield return file;
                            }
                        }
                    }
                }
                else
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
        }

        private static IEnumerable<string> GetAllRazorFiles(this Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (ProjectItem projectItem in project.ProjectItems)
            {
                foreach (string file in GetAllRazorFiles(projectItem))
                {
                    yield return file;
                }
            }
        }

        private static IEnumerable<string> GetAllRazorFiles(this ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem == null)
            {
                yield break;
            }

            if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFile)
            {
                if (projectItem.Name.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                {
                    yield return projectItem.FileNames[0];
                }
            }
            else if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder ||
                     projectItem.Kind == Constants.vsProjectItemKindVirtualFolder ||
                     projectItem.Kind == Constants.vsProjectItemKindSolutionItems)
            {
                if (projectItem.ProjectItems != null)
                {
                    foreach (ProjectItem subItem in projectItem.ProjectItems)
                    {
                        foreach (string file in GetAllRazorFiles(subItem))
                        {
                            yield return file;
                        }
                    }
                }
                if (projectItem.SubProject != null)
                {
                    foreach (string file in GetAllRazorFiles(projectItem.SubProject))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
