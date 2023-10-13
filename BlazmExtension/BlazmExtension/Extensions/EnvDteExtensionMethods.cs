using EnvDTE;
using System.Collections.Generic;
using System.IO;
using BlazmExtension.Singletons;
using Project = EnvDTE.Project;

namespace BlazmExtension.Extensions
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

            var componentNames = new List<string>();
            Projects projects = solution.Projects;
            foreach (Project project in projects)
            {
                foreach (string fileName in ProcessProjectForRazorFiles(project))
                {
                    var componentName = Path.GetFileNameWithoutExtension(fileName);
                    componentNames.Add(componentName);
                    yield return fileName;
                }
            }
            ComponentNameProvider.SetComponentNames(componentNames);
        }

        private static IEnumerable<string> ProcessProjectForRazorFiles(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project.ProjectItems != null)  // Solution folder
            {
                foreach (ProjectItem subProjectItem in project.ProjectItems)
                {
                    foreach (var razorFile in ProcessProjectItemForRazorFiles(subProjectItem))
                    {
                        yield return razorFile;
                    }
                }
            }
        }

        private static IEnumerable<string> ProcessProjectItemForRazorFiles(ProjectItem projectItem)
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
                        foreach (string file in ProcessProjectItemForRazorFiles(subItem))
                        {
                            yield return file;
                        }
                    }
                }

                if (projectItem.SubProject != null)
                {
                    foreach (string file in ProcessProjectForRazorFiles(projectItem.SubProject))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
