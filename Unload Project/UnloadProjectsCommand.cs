using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Linq;

namespace UnloadProjectsExtension
{
    internal sealed class UnloadProjectsCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("1087F425-9225-42F5-8A20-6EA95FC313B3");
        private readonly AsyncPackage package;

        private UnloadProjectsCommand(AsyncPackage package, IMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            new UnloadProjectsCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            var selectedProjects = PromptForProjectNames(dte); // Fix: Pass DTE, not List<string>

            foreach (Project project in dte.Solution.Projects)
            {
                UnloadProjects(project, selectedProjects);
            }
        }

        private void UnloadProjects(Project project, List<string> selectedProjects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (selectedProjects.Contains(project.UniqueName))
                return;

            if (project.Kind != EnvDTE.Constants.vsProjectKindSolutionItems)
            {
                project.DTE.ExecuteCommand("Project.UnloadProject");
            }

            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.SubProject != null)
                {
                    UnloadProjects(item.SubProject, selectedProjects);
                }
            }
        }

        private List<string> PromptForProjectNames(DTE dte)
        {
            var form = new ProjectSelectionForm(dte, new List<string>());
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                return form.SelectedProjects;
            }
            return new List<string>();
        }
    }
}
