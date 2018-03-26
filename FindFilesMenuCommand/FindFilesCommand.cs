using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using EnvDTE;
using System.Collections.Generic;

namespace JamesFrowen.FindFilesMenuCommand
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FindFilesCommand
    {
        private DTE2 dte2;

        private void refreshFileList()
        {
            if (dte2 == null)
            {
                dte2 = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            }

            var solution = dte2.Solution;
            var projects = dte2.Solution.Projects;
            var dir = Path.GetDirectoryName(solution.FileName);
            var sourceDir = Path.Combine(dir, "source");
            foreach (Project proj in projects)
            { 
                if (proj.Name.ToLower().Contains("editor"))
                {
                    addFiles(Path.Combine(sourceDir, "editor"), proj, true);
                }
                else
                {
                    addFiles(sourceDir, proj, false);
                }
            }
        }

        private void addFiles(string dir, Project proj, bool editor)
        {
            var files = getCSFiles(dir, editor);

            foreach (var file in files)
            {
                var item = proj.ProjectItems.AddFromFile(file.FullName);
            }
            proj.Save();
        }

        private FileInfo[] getCSFiles(string dir, bool editor)
        {
            var paths = new List<string>();
            getFilesRecursively(dir, paths, editor);
            var csFiles = new List<FileInfo>();
            foreach (var path in paths)
            {
                var file = new FileInfo(path);
                if (file.Extension == ".cs")
                {
                    csFiles.Add(file);
                }
            }
            return csFiles.ToArray();
        }

        private void getFilesRecursively(string current, List<string> list, bool editor, bool inSideEditorFolder = false)
        {
            foreach (string f in Directory.GetFiles(current))
            {
                list.Add(f);
            }
            foreach (string d in Directory.GetDirectories(current))
            {
                var nextIsEditorFolder = d.ToLower().Contains("editor");
                if (editor && (inSideEditorFolder || nextIsEditorFolder))
                {
                    getFilesRecursively(d, list, true, true);
                }
                else if (!editor && !nextIsEditorFolder)
                {
                    getFilesRecursively(d, list, false, false);
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
