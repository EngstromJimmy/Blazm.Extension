global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlazmExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideUIContextRule(PackageGuids.RazorContextGuidString,
    name: nameof(PackageGuids.RazorContextGuidString),
    expression: "DotRazor",
    termNames: new[] { "DotRazor" },
    termValues: new[] { "HierSingleSelectionName:.razor$" })]
    [Guid(PackageGuids.BlazmExtensionString)]
    public sealed class BlazmExtensionPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
        }
    }
}