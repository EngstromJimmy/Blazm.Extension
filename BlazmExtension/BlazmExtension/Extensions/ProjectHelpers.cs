using EnvDTE;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazmExtension.Extensions
{
    public class ProjectHelpers
    {
        static List<EnvDTE.Project> GetAllProjects(Projects projects)
        {
            List<EnvDTE.Project> allProjects = new List<EnvDTE.Project>();
            foreach (EnvDTE.Project project in projects)
            {
                if (project.Kind == EnvDTE80.ProjectKinds.vsProjectKindSolutionFolder)
                {
                    // This is a solution folder, need to look inside it
                    foreach (EnvDTE.ProjectItem projectItem in project.ProjectItems)
                    {
                        EnvDTE.Project subProject = projectItem.Object as EnvDTE.Project;
                        if (subProject != null)
                        {
                            allProjects.AddRange(GetAllProjects(new List<EnvDTE.Project> { subProject }));
                        }
                    }
                }
                else
                {
                    // This is a real project, add it to the list
                    allProjects.Add(project);
                }
            }
            return allProjects;
        }

        static List<EnvDTE.Project> GetAllProjects(List<EnvDTE.Project> projects)
        {
            List<EnvDTE.Project> allProjects = new List<EnvDTE.Project>();
            foreach (EnvDTE.Project project in projects)
            {
                if (project.Kind == EnvDTE80.ProjectKinds.vsProjectKindSolutionFolder)
                {
                    // This is a solution folder, need to look inside it
                    foreach (EnvDTE.ProjectItem projectItem in project.ProjectItems)
                    {
                        EnvDTE.Project subProject = projectItem.Object as EnvDTE.Project;
                        if (subProject != null)
                        {
                            allProjects.AddRange(GetAllProjects(new List<EnvDTE.Project> { subProject }));
                        }
                    }
                }
                else
                {
                    // This is a real project, add it to the list
                    allProjects.Add(project);
                }
            }
            return allProjects;
        }


        public static List<ComponentItem> GetAllRazorComponentsFromAssembly(EnvDTE.Solution solution)
        {
            List<ComponentItem> items = new List<ComponentItem>();
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Projects projects = solution.Projects;
                foreach (EnvDTE.Project project in GetAllProjects(projects))
                {

                    var activeProject = project;
                    var name = activeProject;
                    string outputDir = activeProject?.ConfigurationManager?.ActiveConfiguration?.Properties?.Item("OutputPath")?.Value?.ToString() ?? "";
                    string outputFileName = activeProject?.Properties?.Item("OutputFileName")?.Value?.ToString() ?? "";
                    string projectDir = activeProject?.Properties?.Item("FullPath")?.Value?.ToString() ?? "";

                    string fullPath = Path.Combine(projectDir, outputDir, outputFileName);

                    if (File.Exists(fullPath))
                    {
                        var readerParameters = new ReaderParameters { ReadSymbols = true };
                        using (var ass = Mono.Cecil.AssemblyDefinition.ReadAssembly(fullPath, readerParameters))
                        {
                            try
                            {
                                var types = ass.MainModule
                                .Types
                                .Where(_ => _.BaseType != null && _.BaseType.FullName == "Microsoft.AspNetCore.Components.ComponentBase");

                                foreach (var t in types)
                                {
                                    var m = t.Methods.FirstOrDefault(_ => _.DebugInformation.HasSequencePoints);
                                    var a = t.CustomAttributes.Where(_ => _.AttributeType.Name == "RouteAttribute");

                                    string sourceFile = "";
                                    //Find the line source file and line number
                                    if (m != null)
                                    {
                                        var line = m.DebugInformation.SequencePoints.FirstOrDefault();
                                        if (line != null)
                                        {
                                            //If the source file is a cs file, see if there is a .razor file.
                                            //If so, use that instead
                                            if (line.Document.Url.EndsWith(".razor.cs"))
                                            {
                                                var razorFile = line.Document.Url.TrimEnd(".cs".ToCharArray());
                                                if (File.Exists(razorFile))
                                                {
                                                    sourceFile = razorFile;
                                                }
                                                else
                                                {
                                                    sourceFile = line.Document.Url;
                                                }
                                            }
                                            else
                                            {
                                                sourceFile = line.Document.Url;
                                            }
                                        }
                                    }

                                    var item = new ComponentItem();

                                    item.Name = t.Name;
                                    item.TypeDefinition = t;
                                    item.FullName = t.FullName;
                                    item.Path = sourceFile;
                                    items.Add(item);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return items;
        }

    }
    public class ComponentItem
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public TypeDefinition TypeDefinition { get; set; }
    }
}
