using System;
using System.Collections.Generic;
using System.Linq;
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

        private ExcessDbContext _db = new ExcessDbContext();
    }
}