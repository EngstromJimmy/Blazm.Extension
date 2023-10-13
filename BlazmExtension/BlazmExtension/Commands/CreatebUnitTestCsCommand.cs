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

            var items = ProjectHelpers.GetAllRazorComponentsFromAssembly(dte.Solution);
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
            sb.AppendLine($"        public void {type.Name}Test()");
            sb.AppendLine("        {");
            sb.AppendLine($"            //Arrange");
            //Add injects
            sb.Append(type.GetInjectDeclarations());
            sb.Append(type.GetParameterDeclarations(false));

            sb.Append($"            var cut = RenderComponent<{type.Name}>(");
            if (type.GetParameters().Any())
            {
                sb.AppendLine("parameters => parameters");
                //Todo: Add Childcontent parameters and RenderFragments


                foreach (var property in type.GetParameters())
                {
                    if (property.Name == "ChildContent")
                    {
                        sb.AppendLine($"                .AddChildContent({TypeDefinitionExtensions.FirstCharToLowerCase(property.Name)})");
                    }
                    else
                    {
                        sb.AppendLine($"                .Add(p => p.{property.Name}, {TypeDefinitionExtensions.FirstCharToLowerCase(property.Name)})");
                    }  
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
    }
}
