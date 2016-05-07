using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Excess.VS
{
    public class InitScriptWizard : IWizard
    {
        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        { }

        public void ProjectFinishedGenerating(Project project)
        {
            //install packages, should revise
            var packageFile = project
                .ProjectItems
                .Item("xs.packages");

            if (packageFile != null)
            {
                var packages = File.ReadAllLines(packageFile.FileNames[0]);
                foreach (var package in packages)
                {
                    var info = package.Split(' ');
                    if (info.Length != 2)
                        continue; //td: error

                    try
                    {
                        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                        IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();

                        var installer = componentModel.GetService<IVsPackageInstaller>();
                        installer.InstallPackage("All", project, info[0], info[1], false);
                    }
                    catch (Exception ex)
                    {
                        //td: error
                    }
                }

                packageFile.Delete();
            }
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {}
        public bool ShouldAddProjectItem(string filePath) { return true; }
        public void BeforeOpeningFile(ProjectItem projectItem) {}
        public void RunFinished() { } 
    }
}
