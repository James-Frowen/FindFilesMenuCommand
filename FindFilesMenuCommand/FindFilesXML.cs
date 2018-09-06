using System;
using System.IO;
using EnvDTE;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace JamesFrowen.FindFilesMenuCommand
{
    [Serializable]
    public class FindFilesXML
    {
        public Project[] projects;

        public Project this[string name]
        {
            get
            {
                foreach (var project in this.projects)
                {
                    if (project.name == name)
                    {
                        return project;
                    }
                }
                throw new KeyNotFoundException();
            }
        }
        public bool ProjectExists(string name)
        {
            foreach (var project in this.projects)
            {
                if (project.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        [Serializable]
        public class Project
        {
            public bool enabled;
            public string name;
            [XmlArrayItem("folder")]
            public string[] folders;
            [XmlArrayItem("match")]
            public string[] matches;
        }

        public static string GetBasePath(Solution solution)
        {
            return Path.GetDirectoryName(solution.FileName);
        }
        private static string getPath(Solution solution)
        {
            var dir = GetBasePath(solution);
            return Path.Combine(dir, FILE_PATH);
        }

        const string FILE_PATH = "AutoFindFiles.xml";
        public static bool Load(Solution solution, out FindFilesXML findFiles)
        {
            string path = getPath(solution);
            findFiles = null;
            if (!File.Exists(path)) { return false; }
            using (var steam = new StreamReader(path))
            {
                try
                {
                    var x = new XmlSerializer(typeof(FindFilesXML));
                    findFiles = x.Deserialize(steam) as FindFilesXML;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Could Not Deserialize FindFilesXML\n" + e.Message);
                    return false;
                }
            }
            return findFiles != null;
        }
        public static void Save(Solution solution, FindFilesXML findFiles)
        {
            string path = getPath(solution);
            using (var steam = new StreamWriter(path))
            {
                var x = new XmlSerializer(typeof(FindFilesXML));
                x.Serialize(steam, findFiles);
            }
        }


        public static FindFilesXML CreateNew(Solution solution)
        {
            var projects = solution.Projects;
            var findFiles = new FindFilesXML()
            {
                projects = new Project[projects.Count]
            };
            int i = 0;
            foreach (EnvDTE.Project proj in projects)
            {
                findFiles.projects[i] = new Project
                {
                    name = proj.Name,
                    enabled = false,
                    folders = new string[] { "./" },
                    matches = new string[] { "*.cs" }
                };

                i++;
            }
            Save(solution, findFiles);

            return findFiles;
        }
    }
}
