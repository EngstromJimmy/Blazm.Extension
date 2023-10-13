using BlazmExtension.Extensions;
using Community.VisualStudio.Toolkit;
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
    [Command(PackageIds.CreatebUnitTestRazor)]
    internal sealed class CreatebUnitTestRazorCommand : BaseCommand<CreatebUnitTestRazorCommand>
    {
        public bool RenderAsRazor = true;
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
            sb.AppendLine($"@inherits TestContext");
            sb.AppendLine($"@using Bunit");
            sb.AppendLine($"@using {type.Namespace};");

            sb.AppendLine($"@code");
            sb.AppendLine("    {");
            sb.AppendLine($"        [Fact]");
            sb.AppendLine($"        public void {type.Name}Test()");
            sb.AppendLine("        {");
            sb.AppendLine($"            //Arrange");
            sb.AppendLine(type.GetInjectDeclarations());
            sb.AppendLine(type.GetParameterDeclarations());

            sb.Append($"            var cut = Render(@");
            foreach (var property in type.GetCascadingParameters())
            {
                sb.AppendLine($"<CascadingValue Value=\"@{TypeDefinitionExtensions.FirstCharToLowerCase(property.Name)}\">");
                sb.Append("                ");
            }
    
            sb.AppendLine($"<{type.Name}");
            foreach (var property in type.GetParametersWithoutRenderFragments())
            {
                sb.Append("                  ");
                sb.AppendLine($"{property.Name}=\"@{TypeDefinitionExtensions.FirstCharToLowerCase(property.Name)}\"");
            }
            sb.AppendLine("                  >");

            //Add all Renderfragments
            foreach (var property in type.GetParametersRenderFragments())
            {
                sb.Append("                  ");
                sb.AppendLine($"<{property.Name}>");
                sb.Append($"<b>{property.Name} fragment</b>");
                sb.AppendLine($"</{property.Name}>");
            }
            sb.AppendLine($"                  </{type.Name}>");
            foreach (var property in type.GetCascadingParameters())
            {
                sb.Append("                  ");
                sb.Append($"</CascadingValue>");
            }
            sb.Append($");");
            sb.AppendLine($"");
            sb.AppendLine($"            //Act");
            sb.AppendLine($"");
            sb.AppendLine($"            //Assert");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            return sb.ToString();
        }
    }
}
