global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using BlazmExtension.Dialogs.Routing;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlazmExtension
{
    [ProvideToolWindow(typeof(RoutingWindow))]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideUIContextRule(PackageGuids.RazorContextGuidString,
    name: nameof(PackageGuids.RazorContextGuidString),
    expression: "DotRazor",
    termNames: new[] { "DotRazor" },
    termValues: new[] { "HierSingleSelectionName:.razor$" })]
    [ProvideUIContextRule(PackageGuids.RazorCsContextGuidString,
    name: nameof(PackageGuids.RazorCsContextGuidString),
    expression: "DotRazorcs",
    termNames: new[] { "DotRazorcs" },
    termValues: new[] { "HierSingleSelectionName:.razor.cs$" })]
    [Guid(PackageGuids.BlazmExtensionString)]
    public sealed class BlazmExtensionPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
        }
    }
}