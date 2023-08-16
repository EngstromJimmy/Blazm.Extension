using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlazmExtension.ExtensionMethods;
using Path = System.IO.Path;
using TextSelection = EnvDTE.TextSelection;

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

                var componentReferences = FindComponentReferences(componentName);

                foreach (var componentReferenceItem in componentReferences)
                {
                    ComponentReferenceDataList.Add(componentReferenceItem);
                }

                ComponentReferenceGrid.ItemsSource = ComponentReferenceDataList;

                ReferenceTextBlock.Text = $"<{componentName}> {componentReferences.Count} references found.";
            }
            catch (Exception ex) { }
        }

        private List<ComponentReferenceItem> FindComponentReferences(string componentName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var razorFiles = _dte.Solution.GetAllRazorFiles();
            var usages = new List<ComponentReferenceItem>();

            var results = razorFiles.Select(file => FindUsagesInFile(componentName, file));

            foreach (var result in results)
            {
                foreach (var componentUsage in result)
                {
                    if (componentUsage != null)
                    {
                        usages.Add(componentUsage);
                    }
                }
            }

            return usages;
        }

        private List<ComponentReferenceItem> FindUsagesInFile(string componentName, string filePath)
        {
            var usages = new List<ComponentReferenceItem>();

            // Read the file line by line
            var lines = File.ReadLines(filePath);

            int lineNumber = 0;
            foreach (var line in lines)
            {
                lineNumber++;  // Keep track of the current line number

                // Check if the line contains the component's usage
                if (line.Contains($"<{componentName}"))
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

            ComponentReferenceItem item = (ComponentReferenceItem)row.DataContext;
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
    public class ComponentReferenceItem
    {
        public string ComponentName { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public string Preview { get; set; }
        public int LineNumber { get; set; }
    }
}
