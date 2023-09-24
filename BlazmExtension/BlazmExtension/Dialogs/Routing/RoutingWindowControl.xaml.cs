using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Assemblies;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using BlazmExtension.Extensions;

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
                string rowText;
                IEnumerable<string> razorFiles = dte.Solution.GetAllRazorFiles();   

                foreach (string file in razorFiles)
                {
                    if (!RoutingDataList.Any(_ => _.Name == file))
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
            
        }
        catch (Exception ex)
        {
            
        }
        RoutingGrid.ItemsSource = RoutingDataList.OrderBy(i => i.Content);
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
        var searchtext = textbox.Text;
        if (searchtext != null)
        {
            RoutingGrid.ItemsSource = RoutingDataList.Where(i => i.Content.Contains(searchtext)).OrderBy(i => i.Name);
        }
    }


    public static List<RoutingItem> GetAllRoutesFromAssembly(EnvDTE.Solution solution)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        List<RoutingItem> items = new List<RoutingItem>();
        Projects projects = solution.Projects;
        foreach (EnvDTE.Project project in projects)
        {

            var activeProject = project;
            string outputDir = activeProject?.ConfigurationManager?.ActiveConfiguration?.Properties?.Item("OutputPath")?.Value?.ToString()??"";
            string outputFileName = activeProject?.Properties?.Item("OutputFileName")?.Value?.ToString()??"";
            string projectDir = activeProject?.Properties?.Item("FullPath")?.Value?.ToString()??"";

            string fullPath = Path.Combine(projectDir, outputDir, outputFileName);

            if (File.Exists(fullPath))
            {
                var readerParameters = new ReaderParameters { ReadSymbols = true };
                var types = Mono.Cecil.AssemblyDefinition
                    .ReadAssembly(fullPath, readerParameters)
                    .MainModule
                    .Types
                    .Where(_ => _.IsPublic && _.CustomAttributes.Any(ca => ca.AttributeType.Name == "RouteAttribute"));

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
                    if (!string.IsNullOrEmpty(sourceFile))
                    {
                        foreach (var item in a)
                        {
                            var routingitem = new RoutingItem();
                            var route = item.ConstructorArguments[0].Value.ToString();
                            routingitem.Name = sourceFile;
                            routingitem.Content = route;
                            items.Add(routingitem);
                        }
                    }
                }
            }
        }
        return items;
    }

    private static string GetRouteFromComponent(Type component)
    {
        var attributes = component.GetCustomAttributes(inherit: true);

        var routeAttribute = attributes.Where(a => a.GetType().Name == "RouteAttribute").FirstOrDefault();

        if (routeAttribute is null)
        {
            // Only map routable components
            return null;
        }

        var route = routeAttribute.GetType().GetProperty("Template").GetValue(routeAttribute, null).ToString();

        //if (string.IsNullOrEmpty(route))
        //{
        //    throw new Exception($"RouteAttribute in component '{component}' has empty route template");
        //}


        return route;
    }

}