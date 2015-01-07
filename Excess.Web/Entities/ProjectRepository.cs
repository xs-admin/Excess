using Excess.Web.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Excess.Web.Entities
{
    public class ProjectRepository
    {
        public IEnumerable<Project> GetUserProjects(string userId)
        {
            return from   project in _db.Projects
                   where  project.UserID == userId
                   select project;
        }

        public IEnumerable<Project> GetSampleProjects()
        {
            return from   project in _db.Projects
                   where  project.IsSample == true
                   select project;
        }

        public Project LoadProject(int projectId)
        {
            var projects = from project in _db.Projects
                           where project.ID == projectId
                           select project;

            var result = projects.FirstOrDefault();
            if (result != null)
                LoadProject(result);

            return result;
        }

        public void LoadProject(Project project)
        {
            var files = from   projectFile in _db.ProjectFiles
                        where  projectFile.OwnerProject == project.ID
                        select projectFile;

            project.ProjectFiles = new List<ProjectFile>(files); 
        }

        public void AddFile(Project project, string name, string contents)
        {
            var file = new ProjectFile
            {
                Name = name,
                Contents = contents,
                OwnerProject = project.ID
            };

            _db.ProjectFiles.Add(file);
            project.ProjectFiles.Add(file);
        }

        public void SaveProject(Project project)
        {
            _db.SaveChanges();
        }

        public void SaveFile(int fileId, string contents)
        {
            var files = from projectFile in _db.ProjectFiles
                        where projectFile.ID == fileId
                        select projectFile;
            var file = files.FirstOrDefault();
            if (file != null)
            {
                file.Contents = contents;
                _db.SaveChanges();
            }
        }

        public int CreateFile(int projectId, string fileName, string contents)
        {
            var newFile = new ProjectFile
            {
                Name         = fileName,
                Contents     = contents,
                OwnerProject = projectId
            };

            _db.ProjectFiles.Add(newFile);
            _db.SaveChanges();

            return newFile.ID; 
        }

        private class DSLConfiguration
        {
            public string parser { get; set; }
            public string linker { get; set; }
            public bool extendsNamespaces { get; set; }
            public bool extendsTypes { get; set; }
            public bool extendsMembers { get; set; }
            public bool extendsCode { get; set; }
        }

        public Project CreateProject(string projectType, string projectName, string projectData, string userId)
        {
            var project = new Project
            {
                IsSample    = false,
                Name        = projectName,
                ProjectType = projectType,
                UserID      = userId
            };

            List<ProjectFile> files = new List<ProjectFile>();
            switch (projectType)
            {
                case "console":
                {
                    files.Add(new ProjectFile { Name = "application", Contents = ProjectTemplates.ConsoleApplication });
                    break;
                }

                case "dsl":
                {
                    DSLConfiguration config = JsonConvert.DeserializeObject<DSLConfiguration>(projectData);

                    //td: parser and linker types
                    StringBuilder members = new StringBuilder();
                    if (config.extendsNamespaces)
                        members.AppendLine(ProjectTemplates.DSLParseNamespace);
                    if (config.extendsTypes)
                        members.AppendLine(ProjectTemplates.DSLParseType);
                    if (config.extendsMembers)
                        members.AppendLine(ProjectTemplates.DSLParseMember);
                    if (config.extendsCode)
                        members.AppendLine(ProjectTemplates.DSLParseCode);

                    files.Add(new ProjectFile
                    {
                        Name = "parser",
                        Contents = string.Format(ProjectTemplates.DSLParser, members.ToString()) 
                    });

                    files.Add(new ProjectFile
                    {
                        Name = "linker",
                        Contents = ProjectTemplates.DSLLinker
                    });
                    break;
                }

                default: throw new InvalidOperationException("Invalid project type: " + projectType);
            }

            _db.Projects.Add(project);
            _db.SaveChanges();

            foreach (var file in files)
            {
                file.OwnerProject = project.ID;
                _db.ProjectFiles.Add(file);
            }

            _db.SaveChanges();
            return project;
        }

        private ExcessDbContext _db = new ExcessDbContext();
    }
}