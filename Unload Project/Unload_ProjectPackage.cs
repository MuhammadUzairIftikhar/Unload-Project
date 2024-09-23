using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnloadProjectsExtension;

namespace Unload_Project
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Unload_ProjectPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class Unload_ProjectPackage : AsyncPackage
    {
        public const string PackageGuidString = "89d4e283-19ce-4375-adb6-37d84c070f4e";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Initialize the command
            await UnloadProjectsCommand.InitializeAsync(this);
        }
    }
}
