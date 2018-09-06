using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;

namespace JamesFrowen.FindFilesMenuCommand
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FindFilesCommand
    {
        private DTE2 dte2;
        private const string FULL_PATH = "FullPath";

        private void refreshFileList()
        {
            if (dte2 == null)
            {
                dte2 = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            }

            var solution = dte2.Solution;
            FindFilesXML findFiles;
            if (!FindFilesXML.Load(solution, out findFiles))
            {
                findFiles = FindFilesXML.CreateNew(solution);
                MessageBox.Show("Created new FindFilesXML\nPlease edit it and run again");
                return;
            }

            var projects = dte2.Solution.Projects;
            var basePath = FindFilesXML.GetBasePath(solution);

            MessageBox.Show("Starting Auto find files");

            foreach (Project proj in projects)
            {
                if (!findFiles.ProjectExists(proj.Name))
                {
                    MessageBox.Show(string.Format("Project with name '{0}' does not exist", proj.Name));
                    continue;
                }
                var data = findFiles[proj.Name];
                if (data.enabled)
                {
                    refreshProject(data, proj, basePath);
                }
            }

            MessageBox.Show("Finished Auto find files");
        }

        private void refreshProject(FindFilesXML.Project data, Project proj, string basePath)
        {
            removeItemsThatDontExist(proj);
            findAndAddItemsFromMatch(data, proj, basePath);
            proj.Save();
        }
        
        private static void findAndAddItemsFromMatch(FindFilesXML.Project data, Project proj, string basePath)
        {
            foreach (var folder in data.folders)
            {
                var fullPath = Path.Combine(basePath, folder);
                foreach (var match in data.matches)
                {
                    var allFiles = Directory.GetFiles(fullPath, match, SearchOption.AllDirectories);
                    foreach (var file in allFiles)
                    {
                        string fullName = Path.GetFullPath(file);
                        proj.ProjectItems.AddFromFile(fullName);
                    }
                }
            }
        }

        private static void removeItemsThatDontExist(Project proj)
        {
            foreach (ProjectItem item in proj.ProjectItems)
            {
                var itemPath = item.Properties.Item(FULL_PATH).Value as string;
                if (!File.Exists(itemPath))
                {
                    item.Remove();
                }
            }
        }

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7c3a4edd-e4eb-4e10-bb3b-445efa3da0a1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindFilesCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private FindFilesCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.findFilesHanlder, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FindFilesCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new FindFilesCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void findFilesHanlder(object sender, EventArgs e)
        {
            this.refreshFileList();
        }
    }
}
