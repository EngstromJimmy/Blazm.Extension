using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace BlazmExtension
{
    [Command(PackageIds.CreateCodebehind)]
    internal sealed class CreateICodebehindCommand : BaseCommand<CreateICodebehindCommand>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;
            UIHierarchy uih = (UIHierarchy)dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
            Array selectedItems = (Array)uih.SelectedItems;
            foreach (UIHierarchyItem selItem in selectedItems)
            {
                ProjectItem projectItem = selItem.Object as ProjectItem;

                if (projectItem != null)
                {
                    string razorFilePath = projectItem.FileNames[1]; 

                    string fileText = File.ReadAllText(razorFilePath);
                    string razorNamespace = "";
                    List<TypeParamInfo> typeParamInfos = GetTypeParametersWithConstraints(fileText);

                    // A very simple example of parsing the namespace from the Razor file
                    string namespaceDirective = "@namespace";
                    int namespaceIndex = fileText.IndexOf(namespaceDirective);

                    if (namespaceIndex != -1)
                    {
                        int namespaceStart = namespaceIndex + namespaceDirective.Length;

                        // This assumes that the namespace directive is followed by a space, 
                        // and the namespace ends with a newline. Adjust if necessary.
                        int namespaceEnd = fileText.IndexOf(Environment.NewLine, namespaceStart);

                        if (namespaceEnd != -1)
                        {
                            razorNamespace = fileText.Substring(namespaceStart, namespaceEnd - namespaceStart).Trim();
                        }
                        else
                        {
                            // If there's no newline after the namespace directive, just take everything that follows it
                            razorNamespace = fileText.Substring(namespaceStart).Trim();
                        }
                    }



                    if (string.IsNullOrEmpty(razorNamespace))
                    {

                        // Get the path to the project and the file
                        string projectPath = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
                        string filePath = Path.GetDirectoryName(projectItem.FileNames[1]);

                        // Get the default namespace of the project
                        string defaultNamespace = projectItem.ContainingProject.Properties.Item("DefaultNamespace").Value.ToString();

                        // Remove the project path from the file path
                        string relativePath = filePath.Substring(projectPath.Length);

                        // Remove leading directory separator if it exists
                        if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            relativePath = relativePath.Substring(1);
                        }

                        // Replace the directory separator characters with dots
                        string namespaceSuffix = relativePath.Replace(Path.DirectorySeparatorChar, '.');

                        // Concatenate the default namespace with the namespace suffix to get the full namespace
                        razorNamespace = string.IsNullOrEmpty(namespaceSuffix) ? defaultNamespace : $"{defaultNamespace}.{namespaceSuffix}";

                    }

                    string fullPath = projectItem.Properties.Item("FullPath").Value.ToString();

                    var newfilePath = fullPath + ".cs";
                    if (!File.Exists(newfilePath))
                    {
                        StringBuilder content = new StringBuilder();
                        content.AppendLine($"namespace {razorNamespace};");
                        content.Append($"public partial class {Path.GetFileNameWithoutExtension(fullPath)}");
                        if (typeParamInfos is { Count: > 0 })
                        {
                            var typeParamsStr = $"<{string.Join(", ", typeParamInfos.Select(x => x.Name))}>";
                            var typeParamConstraints = string.Join(" ", typeParamInfos.Select(x => x.Constraint).Where(x => !string.IsNullOrWhiteSpace(x)));

                            content.Append(typeParamsStr);
                            if (!string.IsNullOrWhiteSpace(typeParamConstraints))
                            {
                                content.Append($" {typeParamConstraints}");
                            }
                        }
                        content.AppendLine();
                        content.AppendLine("{");
                        content.AppendLine("");
                        content.AppendLine("}");

                        File.WriteAllText(newfilePath, content.ToString());
                    }
                }
            }
        }

        private List<TypeParamInfo> GetTypeParametersWithConstraints(string fileStr)
        {
            Regex regex = new Regex(@"@typeparam\s+(\w+)\s*(where\s+\w+\s*:\s*\w+)*");
            MatchCollection matches = regex.Matches(fileStr);

            List<TypeParamInfo> typeParamtersWithConstraints = new ();
            foreach (Match match in matches)
            {
                string typeParam = match.Groups[1].Value;
                string constraint = match.Groups[2].Value;

                
                typeParamtersWithConstraints.Add(new (typeParam, constraint));
            }
            return typeParamtersWithConstraints;
        }

        internal sealed record TypeParamInfo(string Name, string Constraint);
    }
}
