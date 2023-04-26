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

namespace BlazmExtension
{
    [Command(PackageIds.MoveNamespace)]
    internal sealed class MoveNamespaceCommand : BaseCommand<MoveNamespaceCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = await Package.GetServiceAsync(typeof(DTE)) as DTE2;

            var document = dte.ActiveDocument;
            var selection = (TextSelection)dte.ActiveDocument.Selection;

                var usings = ExtractUsings(selection.Text);
                if (usings.Count > 0)
                {
                    var importsPath = FindImportsPath(document.FullName);
                    if (importsPath != null)
                    {
                        AddUsingsToImportsFile(usings, importsPath);
                        selection.Text = "";
                    }
                    else
                    {
                    VsShellUtilities.ShowMessageBox(
                        Package,
                        "Unable to locate the _Imports.razor file.",
                        "Error",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                }
            }

        private static List<string> ExtractUsings(string text)
        {
            var regex = new Regex(@"^@using\s+.+\s*$", RegexOptions.Multiline);
            var matches = regex.Matches(text);

            return matches.Cast<Match>().Select(m => m.Value.Trim()).ToList();
        }

        private static string FindImportsPath(string documentPath)
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(documentPath));
            string importsPath;

            while (directory != null)
            {
                importsPath = Path.Combine(directory.FullName, "_Imports.razor");
                if (File.Exists(importsPath))
                {
                    return importsPath;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static void AddUsingsToImportsFile(List<string> usings, string importsPath)
        {
            var existingUsings = File.ReadAllLines(importsPath).ToList();

            foreach (var usingDirective in usings)
            {
                if (!existingUsings.Contains(usingDirective))
                {
                    existingUsings.Add(usingDirective);
                }
            }

            File.WriteAllLines(importsPath, existingUsings);
        }

        private static void RemoveUsingsFromBuffer(List<string> usings, ITextBuffer textBuffer, ITextEdit edit, int start, int end)
        {
            foreach (var usingDirective in usings)
            {
                var usingSpan = new SnapshotSpan(textBuffer.CurrentSnapshot, start, end - start);
                var usingLine = usingSpan.GetText().IndexOf(usingDirective, StringComparison.Ordinal);

                if (usingLine >= 0)
                {
                    var lineStart = usingSpan.Start + usingLine;
                    var lineEnd = lineStart + usingDirective.Length;
                    edit.Delete(lineStart, lineEnd - lineStart);
                }
            }
        }
    }
}
