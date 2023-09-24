using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlazmExtension.Extensions;
using Path = System.IO.Path;
using TextSelection = EnvDTE.TextSelection;
using System.Text.RegularExpressions;
using BlazmExtension.Singletons;

namespace BlazmExtension.Dialogs.ComponentReferences
{
    /// <summary>
    /// Interaction logic for ComponentReferencesControl.xaml
    /// </summary>
    public partial class ComponentReferencesControl : UserControl
    {
        public ObservableCollection<ComponentReferenceItem> ComponentReferenceDataList { get; set; } = new ();
        private readonly DTE _dte;

        public ComponentReferencesControl()
        {
            InitializeComponent();
            ThreadHelper.ThrowIfNotOnUIThread();
            if (ServiceProvider.GlobalProvider.GetService(typeof(DTE)) is DTE service)
            {
                _dte = service;
            }
        }

        public void Initialize(string componentName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                ComponentReferenceDataList.Clear();

                ReferenceTextBlock.Text = $"Searching for <{componentName}> usages.";

                ComponentReferenceGrid.ItemsSource = ComponentReferenceDataList;

                foreach (var componentReference in FindComponentReferences(componentName))
                {
                    ComponentReferenceDataList.Add(componentReference);
                }

                ReferenceTextBlock.Text = $"<{componentName}> {ComponentReferenceDataList.Count} usages found.";
            }
            catch (Exception ex) { }
        }

        private IEnumerable<ComponentReferenceItem> FindComponentReferences(string componentName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var razorFiles = _dte.Solution.GetAllRazorFiles();


            foreach (var razorFile in razorFiles)
            {
                var results = FindUsagesInFile(componentName, razorFile);
                foreach (var componentUsage in results)
                {
                    if (componentUsage != null)
                    {
                        yield return componentUsage;
                    }
                }
            }
        }

        private Dictionary<string, Regex> _regexCache = new ();
        private List<ComponentReferenceItem> FindUsagesInFile(string componentName, string filePath)
        {
            var usages = new List<ComponentReferenceItem>();

            // Read the file line by line
            var lines = File.ReadLines(filePath);

            if (!_regexCache.TryGetValue(componentName, out var regex))
            {
                var pattern = $@"<{componentName}(?![a-zA-Z])";
                regex = new Regex(pattern, RegexOptions.Compiled);
                _regexCache[componentName] = regex;
            }

            int lineNumber = 0;
            foreach (var line in lines)
            {
                lineNumber++;  // Keep track of the current line number

                // Check if the line contains the component's usage
                if (regex.IsMatch(line))
                {
                    usages.Add(new ComponentReferenceItem()
                    {
                        ComponentName = componentName,
                        FilePath = filePath,
                        LineNumber = lineNumber,
                        Preview = line.Trim()  // Assuming the entire line is what you want as a preview
                    });
                }
            }

            return usages;
        }

        private void ComponentReferenceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow row)) return;

            if (row.DataContext is ComponentReferenceItem item)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                if (dte != null)
                {
                    var window = dte.ItemOperations.OpenFile(item.FilePath);
                    if (window != null)
                    {
                        TextSelection ts = (TextSelection)window.Document.Selection;
                        ts?.GotoLine(item.LineNumber);
                    }
                }
            }
        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string componentName = ComponentSearchTextBox.Text;

            if (string.IsNullOrWhiteSpace(componentName))
            {
                return;
            }

            Initialize(componentName);
        }

        private void ComponentSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void ComponentSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderTextBlock.Visibility = string.IsNullOrEmpty(ComponentSearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private bool handlingTextChange = false;
        private bool isDeletion = false;

        private void ComponentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!handlingTextChange && !isDeletion)
            {
                handlingTextChange = true;  // Flag to indicate we're in the midst of handling

                // Your logic to find the closest matching component name
                string input = ComponentSearchTextBox.Text;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    ComponentSearchTextBox_GotFocus(null, null);
                }
                var matchingComponent = ComponentNameProvider.GetComponentNames().OrderBy(x => x.Length).FirstOrDefault(c => c.StartsWith(input, StringComparison.OrdinalIgnoreCase));

                if (matchingComponent != null)
                {
                    // Unhook the TextChanged event
                    ComponentSearchTextBox.TextChanged -= ComponentSearchTextBox_TextChanged;

                    // Update the TextBox's text and selection
                    ComponentSearchTextBox.Text = matchingComponent;
                    ComponentSearchTextBox.Select(input.Length, matchingComponent.Length - input.Length);

                    // Rehook the TextChanged event
                    ComponentSearchTextBox.TextChanged += ComponentSearchTextBox_TextChanged;
                }

                handlingTextChange = false;  // Reset the flag
            }
            else
            {
                isDeletion = false;  // Reset the deletion flag
            }
        }

        private void ComponentSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && ComponentSearchTextBox.SelectedText.Length > 0)
            {
                // Move the caret to the end and clear the selection
                ComponentSearchTextBox.CaretIndex = ComponentSearchTextBox.Text.Length;
                ComponentSearchTextBox.SelectionLength = 0;

                // Important to set the event as handled
                e.Handled = true;
            }
            else if (e.Key is Key.Back or Key.Delete)
            {
                isDeletion = true;  // Set the deletion flag
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ComponentSearchTextBox.IsFocused)
            {
                Keyboard.ClearFocus();
                ComponentSearchTextBox_LostFocus(null, null);
            }
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ComponentSearchTextBox.IsFocused)
            {
                Keyboard.ClearFocus();
                ComponentSearchTextBox_LostFocus(null, null);
            }
        }
    }
    public class ComponentReferenceItem
    {
        public string ComponentName { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public string Preview { get; set; }
        public int LineNumber { get; set; }
    }
}
