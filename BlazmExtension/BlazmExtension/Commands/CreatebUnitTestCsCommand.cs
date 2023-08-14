using BlazmExtension.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft.ServiceHub.Framework.Services;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;
using static BlazmExtension.Dialogs.Routing.RoutingWindowControl;

namespace BlazmExtension
{
    [Command(PackageIds.CreatebUnitTestCs)]
    internal sealed class CreatebUnitTestCsCommand : BaseCommand<CreatebUnitTestCsCommand>
    {
        public bool RenderAsRazor = false;
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

            var items = GetAllRazorComponentsFromAssembly(dte.Solution);
            foreach (UIHierarchyItem selItem in selectedItems)
            {
                ProjectItem prjItem = selItem.Object as ProjectItem;
                string filePath = prjItem.Properties.Item("FullPath").Value.ToString();

                var type = items.FirstOrDefault(c => c.Path == filePath);
                if (type == null)
                {
                    type = items.FirstOrDefault(c => c.Name == Path.GetFileNameWithoutExtension(filePath));
                }

                if (type == null)
                {
                    System.Windows.Forms.MessageBox.Show("Could not find component, you can try to build you project first");
                }

                //Generate bUnit test in xUnit based on type name, inject, etc
                var test = GenerateTest(type.TypeDefinition);
                Clipboard.SetText(test);
            }


        }



       

        public string GenerateTest(TypeDefinition type)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"    public class {type.Name}Tests : TestContext");
            
            sb.AppendLine("    {");
            sb.AppendLine($"        [Fact]");
            sb.AppendLine($"        public void Test1()");
            sb.AppendLine("        {");
            sb.AppendLine($"            //Arrange");
            //Add injects
            sb.Append(type.GetInjectDeclarations());
            sb.Append(type.GetParameterDeclarations());

            sb.Append($"            var cut = RenderComponent<{type.Name}>(");
            if (type.GetParameters().Any())
            {
                sb.AppendLine("parameters => parameters");
                foreach (var property in type.GetParameters())
                {
                        sb.AppendLine($"                .Add(p => p.{property.Name}, {TypeDefinitionExtensions.FirstCharToLowerCase(property.Name)})");
                        
                }
                foreach (var property in type.GetCascadingParameters())
                {
                    sb.AppendLine($"                .Add(p => p.{property.Name}, {TypeDefinitionExtensions.FirstCharToLowerCase(property.Name)})");

                }
            }
                sb.AppendLine($"            );");
            
            //Handle EventCallbacks
            //Handle ChildContent
            //Handle RenderFragments
            //Handle RenderFragment<T>



            

            sb.AppendLine($"            //Act");
            sb.AppendLine($"");
            sb.AppendLine($"            //Assert");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            return sb.ToString();
        }

        public class ComponentItem
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public string Path { get; set; }
            public TypeDefinition TypeDefinition { get; set; }
        }

        public static List<ComponentItem> GetAllRazorComponentsFromAssembly(EnvDTE.Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<ComponentItem> items = new List<ComponentItem>();
            Projects projects = solution.Projects;
            foreach (EnvDTE.Project project in projects)
            {

                var activeProject = project;
                string outputDir = activeProject?.ConfigurationManager?.ActiveConfiguration?.Properties?.Item("OutputPath")?.Value?.ToString() ?? "";
                string outputFileName = activeProject?.Properties?.Item("OutputFileName")?.Value?.ToString() ?? "";
                string projectDir = activeProject?.Properties?.Item("FullPath")?.Value?.ToString() ?? "";

                string fullPath = Path.Combine(projectDir, outputDir, outputFileName);

                if (File.Exists(fullPath))
                {
                    var readerParameters = new ReaderParameters { ReadSymbols = true };
                    using (var ass = Mono.Cecil.AssemblyDefinition.ReadAssembly(fullPath, readerParameters))
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
                }
            }
            return items;
        }
    }
}
