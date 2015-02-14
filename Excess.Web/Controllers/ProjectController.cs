using Excess.RuntimeProject;
using Excess.Web.Entities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Excess.Web.Controllers
{
    public class ProjectController : ExcessControllerBase
    {
        public ProjectController(IProjectManager manager)
        {
            _manager = manager;
        }

        public ActionResult LoadProject(int projectId)
        {
            ProjectRepository repo = new ProjectRepository();

            dynamic config; 
            Project project = repo.LoadProject(projectId, out config);
            if (project == null)
                return HttpNotFound();

            if (!project.IsSample)
            {
                if (!User.Identity.IsAuthenticated)
                    return HttpNotFound(); //td: right error

                if (User.Identity.GetUserId() != project.UserID)
                    return HttpNotFound(); //td: right error
            }

            IRuntimeProject runtime = _manager.createRuntime(project.ProjectType, project.Name, config);
            foreach (var file in project.ProjectFiles)
                runtime.add(file.Name, file.ID, file.Contents);

            Session["project"] = runtime;

            if (!project.IsSample)
                Session["projectId"] = project.ID;
            else
                Session["SampleProjectId"] = project.ID;

            return Json(new
            {
                defaultFile = runtime.defaultFile(),
                tree        = new[] { projectTree(project, runtime) }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadFile(string file)
        {
            var project = Session["project"] as IRuntimeProject;
            if (project == null)
                return HttpNotFound(); //td: right error

            string contents = project.fileContents(file);
            if (contents == null)
                return HttpNotFound(); //td: right error

            return Content(contents);
        }

        [ValidateInput(false)]
        public ActionResult SaveFile(string file, string contents)
        {
            var project = Session["project"] as IRuntimeProject;
            if (project == null)
                return HttpNotFound(); //td: right error

            project.modify(file, contents);

            if (Session["projectId"] != null)
            {
                int fileIdx = project.fileId(file);
                if (fileIdx < 0)
                    return HttpNotFound(); //td: right error

                ProjectRepository repo = new ProjectRepository();
                repo.SaveFile(fileIdx, contents);
            }

            return Content("ok"); 
        }

        public ActionResult CreateClass(string className)
        {
            var project = Session["project"] as IRuntimeProject;
            if (project == null)
                return HttpNotFound(); //td: right error

            var contents = string.Format("class {0} \n{{\n}}", className);
            var fileId   = -1;
            if (Session["projectId"] != null)
            {
                ProjectRepository repo = new ProjectRepository();
                fileId = repo.CreateFile((int)Session["projectId"], className, contents);
            }

            project.add(className, fileId, contents);
            return Content("ok");
        }

        private class CompilationResult
        {
            public CompilationResult(IEnumerable<Error> errors, dynamic clientData = null)
            {
                Succeded = errors == null || !errors.Any();
                Errors = errors;
                ClientData = clientData;
            }

            public bool Succeded { get; set; }
            public IEnumerable<Error> Errors { get; set; }
            public dynamic ClientData { get; set; }
        }

        public ActionResult Compile()
        {
            var project = Session["project"] as IRuntimeProject;
            if (project == null)
                return HttpNotFound(); //td: right error

            var result = new CompilationResult(project.compile());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Execute()
        {
            var project = Session["project"] as IRuntimeProject;
            if (project == null)
                return HttpNotFound(); //td: right error

            dynamic clientData;
            var errors = project.run(out clientData);
            var result = new CompilationResult(errors, clientData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Notifications()
        {
            var project = Session["project"] as IRuntimeProject;
            if (project == null)
                return HttpNotFound(); //td: right error

            var notifications = project.notifications();
            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserProjects()
        {
            if (!User.Identity.IsAuthenticated)
                return HttpNotFound(); //td: right error

            ProjectRepository repo = new ProjectRepository();
            var projects = repo.GetUserProjects(User.Identity.GetUserId());
            return Json(projects, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateProject(string projectType, string projectName, string projectData)
        {
            if (!User.Identity.IsAuthenticated)
                return HttpNotFound(); //td: right error

            ProjectRepository repo   = new ProjectRepository();
            Project           result = repo.CreateProject(projectType, projectName, projectData, User.Identity.GetUserId());

            return Json(new { projectId = result.ID }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult debugDSL(string text)
        {
            var runtime = Session["project"] as IExtensionRuntime;
            if (runtime == null)
                return HttpNotFound(); //td: right error

            string result;
            try
            {
                result = runtime.debugExtension(text);

                var notProvider = runtime as IRuntimeProject;
                var nots = notProvider.notifications();

                if (nots.Any())
                {
                    StringBuilder notBuilder = new StringBuilder();
                    foreach (var not in nots)
                        notBuilder.AppendLine(not.Message);

                    result = string.Format("{0} \n ================= Notifications ================= \n {1}", 
                        result, notBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(result);
        }

        public ActionResult GetDSLTests()
        {
            var project = Session["projectId"] as int?;
            if (project == null)
                project = Session["SampleProjectId"] as int?;

            if (project == null)
                return HttpNotFound(); //td: right error

            //td: !!! entity project
            ExcessDbContext _db = new ExcessDbContext();
            var result = _db.DSLTests.Where( test => test.ProjectID == project);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(false)]
        public ActionResult UpdateDSLTest(string id, string contents)
        {
            var project = Session["projectId"] as int?;
            if (project == null)
                return HttpNotFound(); //td: right error

            Guid testId;
            if (!Guid.TryParse(id, out testId))
                return HttpNotFound(); //td: right error

            ExcessDbContext _db = new ExcessDbContext();
            var result = _db.DSLTests
                .Where(test => test.ID == testId)
                .FirstOrDefault();

            if (result == null)
                return HttpNotFound(); //td: right error

            result.Contents = contents;
            _db.SaveChanges();

            return Content("ok");
        }

        [ValidateInput(false)]
        public ActionResult AddDSLTest(string name, string contents)
        {
            var project = Session["projectId"] as int?;
            if (project == null)
                return HttpNotFound(); //td: right error

            ExcessDbContext _db = new ExcessDbContext();

            var result = new DSLTest
            {
                ID        = Guid.NewGuid(),  
                ProjectID = (int)project,
                Caption   = name,
                Contents  = contents  
            };

            _db.DSLTests.Add(result);
            _db.SaveChanges();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private TreeNode projectTree(Project project, IRuntimeProject runtime)
        {
            TreeNode result = new TreeNode
            {
                label   = project.Name,
                icon    = "fa-sitemap",
                actions = new[]
                {
                    new TreeNodeAction { id = "add-class", icon = "fa-plus-circle" }
                } ,
                children = project.ProjectFiles
                    .Where(projectFile => !projectFile.isHidden)
                    .Select<ProjectFile, TreeNode>(projectFile =>
                    {
                        return new TreeNode
                        {
                            label = projectFile.Name,
                            icon = "fa-code",
                            action = "select-file",
                            data = projectFile.Name,
                            actions = new[]
                            {
                                new TreeNodeAction { id = "remove-file", icon = "fa-times-circle-o"       },
                                new TreeNodeAction { id = "open-tab",    icon = "fa-arrow-circle-o-right" },
                            }.Union(runtime.fileActions(projectFile.Name))
                        };
                    })
            };

            return result;
        }

        private IProjectManager _manager;
    }
}