namespace Excess.Web.Migrations
{
    using Excess.Web.Entities;
    using Excess.Web.Resources;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Excess.Web.Entities.ExcessDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Excess.Web.Entities.ExcessDbContext context)
        {
            foreach (var sample in TranslationSamples())
                context.Samples.AddOrUpdate(sample);

            foreach (var sampleProject in SampleProjects())
                context.Projects.AddOrUpdate(sampleProject);

            foreach (var file in SampleProjectFiles())
                context.ProjectFiles.AddOrUpdate(file);
        }

        private TranslationSample[] TranslationSamples()
        {
            return new TranslationSample[]
            {
                new TranslationSample
                {
                    ID = 1,
                    Name = "Functions",
                    Contents = SampleCode.Functions,
                },

                new TranslationSample
                {
                    ID = 2,
                    Name = "Events",
                    Contents = SampleCode.Events,
                },

                new TranslationSample
                {
                    ID = 3,
                    Name = "Arrays",
                    Contents = SampleCode.Arrays,
                },

                new TranslationSample
                {
                    ID = 4,
                    Name = "Misc",
                    Contents = SampleCode.Misc,
                },

                new TranslationSample
                {
                    ID = 5,
                    Name = "DSL (Asynch/Synch)",
                    Contents = SampleCode.DSLAsynch,
                },
            };
        }
        private ProjectFile[] SampleProjectFiles()
        {
            return new ProjectFile[]
            {
                new ProjectFile
                {
                    ID = 1,
                    OwnerProject = 1,
                    Name = "application",
                    Contents = SampleCode.HelloWorld,
                },

                new ProjectFile
                {
                    ID = 2,
                    OwnerProject = 2,
                    Name = "application",
                    Contents = SampleCode.LolCatsApplication,
                },

                new ProjectFile
                {
                    ID = 3,
                    OwnerProject = 2,
                    Name = "lolcat",
                    Contents = SampleCode.LolCats,
                },

                new ProjectFile
                {
                    ID = 4,
                    OwnerProject = 2,
                    Name = "speek",
                    Contents = SampleCode.LolCatsSpeek,
                },

                new ProjectFile
                {
                    ID = 5,
                    OwnerProject = 2,
                    Name = "trollcat",
                    Contents = SampleCode.LolCatsTrollcat,
                },
            };
        }

        private Project[] SampleProjects()
        {
            return new Project[]
            {
                new Project
                {
                    ID = 1,
                    ProjectType = "console",
                    Name = "Hello World",
                    IsSample = true,
                },

                new Project
                {
                    ID = 2,
                    ProjectType = "console",
                    Name = "LOL Cats",
                    IsSample = true,
                },
            };
        }
    }
}
