using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Excess.Web.Entities
{
    public class Project
    {
        public int ID { get; set; }

        public string Name { get; set; }
        public string ProjectType { get; set; }
        public bool IsSample { get; set; }
        public string UserID { get; set; }

        [NotMapped]
        public ICollection<ProjectFile> ProjectFiles { get; set; }

        public ProjectFile Find(string file)
        {
            if (ProjectFiles == null)
                return null;

            var result = ProjectFiles.Where(projectFile => projectFile.Name == file);
            if (result == null || !result.Any())
                return null;

            return result.First();
        }
    }
    public class ProjectFile
    {
        public int ID { get; set; }

        public string Name { get; set; }
        public string Contents { get; set; }
        public int OwnerProject { get; set; }
    }
}