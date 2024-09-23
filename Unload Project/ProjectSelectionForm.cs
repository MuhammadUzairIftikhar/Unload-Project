using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;

namespace UnloadProjectsExtension
{
    public partial class ProjectSelectionForm : Form
    {
        public List<string> SelectedProjects { get; private set; }

        public ProjectSelectionForm(DTE dte, List<string> selectedProjects)
        {
            InitializeComponent();
            SelectedProjects = new List<string>(selectedProjects);
            PopulateTreeView(dte);
            MarkSelectedProjects(treeView1.Nodes);
        }

        private void PopulateTreeView(DTE dte)
        {
            treeView1.Nodes.Clear();
            foreach (Project project in dte.Solution.Projects)
            {
                var node = CreateTreeNode(project);
                treeView1.Nodes.Add(node);
            }
        }

        private TreeNode CreateTreeNode(Project project)
        {
            TreeNode node = new TreeNode(project.Name) { Tag = project.UniqueName };

            // Recursively add sub-projects
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.SubProject != null)
                {
                    node.Nodes.Add(CreateTreeNode(item.SubProject));
                }
            }
            return node;
        }

        private void MarkSelectedProjects(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = SelectedProjects.Contains((string)node.Tag);
                if (node.Nodes.Count > 0)
                {
                    MarkSelectedProjects(node.Nodes);
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SelectedProjects.Clear();
            GetCheckedNodes(treeView1.Nodes);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void GetCheckedNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                {
                    SelectedProjects.Add((string)node.Tag);
                }

                if (node.Nodes.Count > 0)
                {
                    GetCheckedNodes(node.Nodes);
                }
            }
        }
    }
}
