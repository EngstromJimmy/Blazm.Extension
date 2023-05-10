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
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace BlazmExtension
{
    [Command(PackageIds.MoveCodebehind)]
    internal sealed class MoveCodebehindToRazorCommand : BaseCommand<MoveCodebehindToRazorCommand>
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
                if ((int)VSConstants.MessageBoxResult.IDOK == VsShellUtilities.ShowMessageBox(
                        Package,
                        "This feature is in beta, please make sure you backup or commit your razor and code-behind code",
                        "Commit or backup your code",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST))
                {
                    ProjectItem prjItem = selItem.Object as ProjectItem;
                    string filePath = prjItem.Properties.Item("FullPath").Value.ToString();


                    if (!filePath.EndsWith(".razor.cs"))
                    {
                        VsShellUtilities.ShowMessageBox(
                            Package,
                            "Please make sure you have selected a Razor code-behind file (.razor.cs) to use this command.",
                            "Error",
                            OLEMSGICON.OLEMSGICON_CRITICAL,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                        return;
                    }

                    var codeBehindPath = filePath;
                    //Remove the ".cs" from the end
                    var razorPath = Path.ChangeExtension(codeBehindPath, "");

                    if (!File.Exists(razorPath))
                    {
                        VsShellUtilities.ShowMessageBox(
                            Package,
                            $"The corresponding Razor file '{razorPath}' does not exist.",
                            "Error",
                            OLEMSGICON.OLEMSGICON_CRITICAL,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                        return;
                    }

                    var codeBehindText = File.ReadAllText(codeBehindPath);

                    SyntaxTree tree = CSharpSyntaxTree.ParseText(codeBehindText);
                    CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                    var members = tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>();

                    StringBuilder methods = new();
                    StringBuilder usings = new();
                    StringBuilder fields = new();
                    StringBuilder properties = new();
                    StringBuilder inject = new();
                    StringBuilder attributes = new();
                    StringBuilder implements = new();
                    StringBuilder inherits = new();
                    StringBuilder pagedirective = new();
                    StringBuilder nestedclasses = new();

                    foreach (var u in root.Usings)
                    {
                        usings.AppendLine($"@{u.ToString().TrimEnd(';')}");
                    }

                    foreach (var member in members)
                    {
                        //Any class attributes
                        var classnode = member as ClassDeclarationSyntax;
                        if (classnode != null)
                        {
                            foreach (var nestedclass in FindNestedClasses(classnode))
                            {
                                nestedclasses.AppendLine(nestedclass.ToString());
                            }
                            
                            if (classnode.BaseList != null)
                            {
                                var list = classnode.BaseList.Types;
                                foreach (var t in list)
                                {
                                    if (list.ToString().StartsWith("I"))
                                    {
                                        implements.AppendLine($"@implements {t.ToString()}");
                                    }
                                    else
                                    {
                                        inherits.AppendLine($"@inherits {t}");
                                    }
                                }
                            }

                            foreach (var al in classnode.AttributeLists)
                            {
                                foreach (var attribute in al.Attributes)
                                {
                                    attributes.AppendLine($"@attribute [{attribute.ToString()}]");
                                }
                            }
                        }

                        //If the members are not from the component class then ignore them
                        var parent = member.Parent as ClassDeclarationSyntax;
                        if (parent != null)
                        {
                            if (parent.Identifier.Text != Path.GetFileNameWithoutExtension(filePath).TrimSuffix(".razor"))
                            {
                                continue;
                            }
                        }


                        var property = member as PropertyDeclarationSyntax;
                        if (property != null)
                        {
                            //If it has an Inject attribute, then we need to add it to the razor file at the top
                            if (HasInjectAttribute(property))
                            {
                                var typename = ((IdentifierNameSyntax)property.Type).Identifier.Value;
                                inject.AppendLine($"@inject {typename} {property.Identifier.Text}");
                            }
                            else
                            {
                                properties.AppendLine(property.ToString());
                            }
                        }

                        var field = member as FieldDeclarationSyntax;
                        if (field != null)
                        {
                            fields.AppendLine(field.ToString());
                        }

                        var method = member as MethodDeclarationSyntax;
                        if (method != null)
                        {
                            methods.AppendLine(method.ToString());
                        }
                    }

                    var razorContent = File.ReadAllText(razorPath);
                    //Page
                    var pageDirectivePattern = new Regex(@"@page\s+(?:""(?<route>[^""]*)""|'(?<route>[^']*)')?", RegexOptions.Multiline);
                    var matches = pageDirectivePattern.Matches(razorContent);
                    if (matches.Count > 0)
                    {
                        //Add the using to the usings sb
                        foreach (Match match in matches)
                        {
                            var block = match.Groups["route"].Value;
                            if (!ContainsLine(pagedirective, block))
                            {
                                pagedirective.AppendLine($"""@page "{block}" """);
                            }
                        }
                        //Remove the usings from the razor file
                        razorContent = pageDirectivePattern.Replace(razorContent, "");
                    }


                    //Usings
                    var usingPattern = new Regex(@"@using\s+(?<content>[\s\S]*?)(\r\n|\r|\n)", RegexOptions.Multiline);
                    var usingMatches = usingPattern.Matches(razorContent);
                    if (usingMatches.Count > 0)
                    {
                        //Add the using to the usings sb
                        foreach (Match usingMatch in usingMatches)
                        {
                            var usingBlock = usingMatch.Groups["content"].Value;
                            var newblock = $"@using {usingBlock}";
                            if (!ContainsLine(usings, newblock))
                            {
                                usings.AppendLine(newblock);
                            }
                        }
                        //Remove the usings from the razor file
                        razorContent = usingPattern.Replace(razorContent, "");
                    }


                    //Injects
                    var injectPattern = new Regex(@"@inject\s+(?<content>[\s\S]*?)(\r\n|\r|\n)", RegexOptions.Multiline);
                    var injectMatches = injectPattern.Matches(razorContent);
                    if (injectMatches.Count > 0)
                    {
                        //Add the using to the inject sb
                        foreach (Match match in injectMatches)
                        {
                            var block = match.Groups["content"].Value;
                            var newblock = $"@inject {block}";
                            if (!ContainsLine(inject, block))
                            {
                                inject.AppendLine(newblock);
                            }
                        }
                        //Remove the usings from the razor file
                        razorContent = injectPattern.Replace(razorContent, "");
                    }



                    //Attributes
                    var attributePattern = new Regex(@"@attribute\s+(?<content>[\s\S]*?)(\r\n|\r|\n)", RegexOptions.Multiline);
                    var attributeMatches = attributePattern.Matches(razorContent);
                    if (attributeMatches.Count > 0)
                    {
                        foreach (Match match in attributeMatches)
                        {
                            var block = match.Groups["content"].Value;
                            var newblock = $"@attribute {block}";
                            if (!ContainsLine(attributes, newblock))
                            {
                                attributes.AppendLine(newblock);
                            }
                        }
                        //Remove the usings from the razor file
                        razorContent = attributePattern.Replace(razorContent, "");
                    }

                    //Implements
                    var implementsPattern = new Regex(@"@implements\s+(?<content>[\s\S]*?)(\r\n|\r|\n)", RegexOptions.Multiline);
                    var implementsMatches = implementsPattern.Matches(razorContent);
                    if (implementsMatches.Count > 0)
                    {
                        foreach (Match match in implementsMatches)
                        {
                            var block = match.Groups["content"].Value;
                            var newblock = $"@implements {block}";
                            if (!ContainsLine(implements, newblock))
                            {
                                implements.AppendLine(newblock);
                            }
                        }
                        //Remove the usings from the razor file
                        razorContent = implementsPattern.Replace(razorContent, "");
                    }

                    //Inherits
                    var inheritsPattern = new Regex(@"@inherits\s+(?<content>[\s\S]*?)(\r\n|\r|\n)", RegexOptions.Multiline);
                    var inheritsMatches = inheritsPattern.Matches(razorContent);
                    if (inheritsMatches.Count > 0)
                    {
                        foreach (Match match in inheritsMatches)
                        {
                            var block = match.Groups["content"].Value;
                            var newblock = $"@inherits {block}";
                            if (!ContainsLine(inherits, newblock))
                            {
                                inherits.AppendLine(newblock);
                            }
                        }
                        razorContent = inheritsPattern.Replace(razorContent, "");
                    }
                    //Remove any new lines at the top of the razor file
                    razorContent = Regex.Replace(razorContent, @"^\s*[\r\n]+", string.Empty, RegexOptions.Multiline);



                    //Add content to razor file
                    var codeBlockPattern = new Regex(@"@code\s*{(?<content>[\s\S]*?)}", RegexOptions.Multiline);
                    var codeMatch = codeBlockPattern.Match(razorContent);
                    var updatedCodeBlock = Environment.NewLine + properties.ToString() + fields.ToString() + methods.ToString() + nestedclasses.ToString();
                    if (codeMatch.Success)
                    {
                        var codeBlock = codeMatch.Groups["content"].Value;
                        updatedCodeBlock = codeBlock + updatedCodeBlock;
                        razorContent = codeBlockPattern.Replace(razorContent, $"@code {{{updatedCodeBlock}}}");
                    }
                    else
                    {
                        razorContent = razorContent + Environment.NewLine + $"@code {{{updatedCodeBlock} }}";
                    }

                    //Add page, usings, injects, attributes, implements, inherits to razor file
                    razorContent = pagedirective.ToString() + usings.ToString() + inject.ToString() + attributes.ToString() + implements.ToString() + inherits.ToString() + razorContent;

                    File.WriteAllText(razorPath, razorContent);
                    File.Delete(codeBehindPath);
                    

                    try
                    {
                        if (System.IO.File.Exists(razorPath))
                        {
                            Window window = dte.ItemOperations.OpenFile(razorPath);
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => {
                                dte.ExecuteCommand("Edit.FormatDocument");
                            }), DispatcherPriority.ApplicationIdle, null);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"File does not exist: {filePath}");
                        }
                    }
                    catch (COMException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error opening and formatting file: {ex.Message}");
                    }
                }
            }
        }

        private static bool HasInjectAttribute(PropertyDeclarationSyntax propertyNode)
        {
            foreach (var attributeList in propertyNode.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (attribute.Name.ToString().EndsWith("Inject"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        bool ContainsLine(StringBuilder sb, string targetLine)
        {
            string[] lines = sb.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return lines.Any(line => line.Trim() == targetLine.Trim());
        }

        private static List<ClassDeclarationSyntax> FindNestedClasses(ClassDeclarationSyntax classDeclaration)
        {
            var nestedClasses = new List<ClassDeclarationSyntax>();

            foreach (var member in classDeclaration.Members)
            {
                if (member is ClassDeclarationSyntax nestedClass)
                {
                    nestedClasses.Add(nestedClass);
                }
            }

            return nestedClasses;
        }
    }
}
