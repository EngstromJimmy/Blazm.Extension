using EnvDTE;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BlazmExtension.Dialogs.Routing;

public partial class RoutingWindowControl : UserControl
{
    public RoutingWindowControl()
    {
        InitializeComponent();
        ThreadHelper.ThrowIfNotOnUIThread();
        if (ServiceProvider.GlobalProvider.GetService(typeof(DTE)) is DTE service)
        {
            dte = service;
            Load();
        }
    }

    private readonly DTE dte;

    private void Load()
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array _projects = dte.ActiveSolutionProjects as Array;
            RoutingDataList.Clear();
            if (_projects.Length != 0 && _projects != null)
            {
                foreach (EnvDTE.Project project in _projects)
                {
                    //get the project path
                    string _directoryPath = new FileInfo(project.FullName).DirectoryName;
                    string rowText;
                    string[] razorFiles = Directory.GetFiles(_directoryPath, "*.razor", SearchOption.AllDirectories);

                    foreach (string file in razorFiles)
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            int row = 0;
                            while (sr.Peek() >= 0)
                            {
                                rowText = sr.ReadLine();

                                if (rowText.Contains("@page") && !rowText.StartsWith("@*"))
                                {
                                    string content = rowText.Replace("@page", "").Replace("\"", "").Trim();
                                    RoutingDataList.Add(new RoutingItem() { Name = file, Content = content });
                                }
                                else if (row > 5)
                                {
                                    break;
                                }

                                row++;
                            }
                        }

                    }
                }
            }
            else
            {
                //RoutingDataList.Add(new RoutingItem() { Name = "No Project in solution or selected" });
            }
            RoutingGrid.ItemsSource = RoutingDataList.OrderBy(i => i.Name);
        }
        catch (Exception ex)
        {
            
        }
    }

    public ObservableCollection<RoutingItem> RoutingDataList { get; set; } = new ObservableCollection<RoutingItem>();

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        Load();
    }

    

    private void RoutingGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!(ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow row))
        {
            return;
        }

        RoutingItem item = (RoutingItem)row.DataContext;
        ThreadHelper.ThrowIfNotOnUIThread();
        if (!string.IsNullOrEmpty(item.Name))
        {
            _ = dte.ItemOperations.OpenFile(item.Name);
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textbox = sender as TextBox;
        var searchtext=textbox.Text;
        if(searchtext != null)
        {
           RoutingGrid.ItemsSource = RoutingDataList.Where(i => i.Content.Contains(searchtext)).OrderBy(i => i.Name);
        }
    }
}