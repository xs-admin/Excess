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

            int projectSamples = 1;
            foreach (var sampleProject in SampleProjects())
            {
                projectSamples++;
                context.Projects.AddOrUpdate(sampleProject);
            }

            reserveProjects(context, projectSamples, 299);

            int projectFileSamples = 1;
            foreach (var file in SampleProjectFiles())
            {
                projectFileSamples++;
                context.ProjectFiles.AddOrUpdate(file);
            }

            reserveProjectFiles(context, projectFileSamples, 999);

            int dslConfigs = 1;
            foreach (var config in DSLProjects())
            {
                dslConfigs++;
                context.DSLProjects.AddOrUpdate(config);
            }

            reserveDSLProjectFiles(context, dslConfigs, 99);

            foreach (var dslTest in DSLTests())
            {
                context.DSLTests.AddOrUpdate(dslTest);
            }
        }

        private void reserveProjectFiles(ExcessDbContext context, int projectFileSamples, int reserveCount)
        {
            for (int i = projectFileSamples; i < reserveCount; i++)
            {
                var reserved = new ProjectFile { ID = i };
                context.ProjectFiles.AddOrUpdate(reserved);
            }
        }

        private void reserveProjects(ExcessDbContext context, int projectSamples, int reserveCount)
        {
            for (int i = projectSamples; i < reserveCount; i++)
            {
                var reserved = new Project { ID = i, IsSample = true };
                context.Projects.AddOrUpdate(reserved);
            }
        }

        private void reserveDSLProjectFiles(ExcessDbContext context, int dslProjects, int reserveCount)
        {
            for (int i = dslProjects; i < reserveCount; i++)
            {
                var reserved = new DSLProject { ID = i };
                context.DSLProjects.AddOrUpdate(reserved);
            }
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
                //Hello world
                new ProjectFile
                {
                    ID = 1,
                    OwnerProject = 1,
                    Name = "application",
                    Contents = SampleCode.HelloWorld,
                },

                //Lolcats
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

                //Pure
                new ProjectFile 
                {
                    ID           = 6,
                    OwnerProject = 3,
                    Name         = "plugin",
                    isHidden     = true,   
                    Contents     = string.Format(ProjectTemplates.DSLPlugin, "pure")
                },

                new ProjectFile
                {
                    ID           = 7,
                    OwnerProject = 3,
                    Name         = "parser",
                    Contents     = SampleCode.PureParser,
                },

                new ProjectFile
                {
                    ID           = 8,
                    OwnerProject = 3,
                    Name         = "linker",
                    Contents     = SampleCode.PureLinker,
                },

                //match
                new ProjectFile
                {
                    ID           = 9,
                    OwnerProject = 4,
                    Name         = "plugin",
                    isHidden     = true,
                    Contents     = string.Format(ProjectTemplates.DSLPlugin, "match")
                },

                new ProjectFile
                {
                    ID           = 10,
                    OwnerProject = 4,
                    Name         = "parser",
                    Contents     = SampleProjectStrings.MatchParser,
                },

                new ProjectFile
                {
                    ID           = 11,
                    OwnerProject = 4,
                    Name         = "linker",
                    Contents     = SampleProjectStrings.MatchLinker,
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

                new Project
                {
                    ID = 3,
                    ProjectType = "dsl",
                    Name = "Pure Class DSL",
                    IsSample = true,
                },

                new Project
                {
                    ID = 4,
                    ProjectType = "dsl",
                    Name = "Match DSL",
                    IsSample = true,
                },
            };
        }

        private DSLProject[] DSLProjects()
        {
            return new DSLProject[]
            {
                new DSLProject
                {
                    ID           = 1,
                    ProjectID    = 3,
                    Name         = "pure",
                    ExtendsTypes = true,
                },
                new DSLProject
                {
                    ID          = 2,
                    ProjectID   = 4,
                    Name        = "match",
                    ExtendsCode = true,
                },
            };

        }

        static Guid PureTest1  = new Guid("E8FB63DB-D135-4FE9-893A-24A4162A1D0B");
        static Guid PureTest2  = new Guid("6C3371F4-59AD-4D74-91BA-50C5A9424632");
        static Guid MatchTest1 = new Guid("834A3BDB-40A4-4025-8588-FB341C22A2E6");

        private DSLTest[] DSLTests()
        {
            return new DSLTest[]
            {
                new DSLTest
                {
                    ID          = PureTest1,
                    ProjectID   = 3,
                    Caption     = "Testing impurity",
                    Contents    = SampleProjectStrings.PureTest1,
                },

                new DSLTest
                {
                    ID          = PureTest2,
                    ProjectID   = 3,
                    Caption     = "Testing static classes",
                    Contents    = SampleProjectStrings.PureTest2,
                },
                new DSLTest
                {
                    ID          = MatchTest1,
                    ProjectID   = 4,
                    Caption     = "Usage",
                    Contents    = SampleProjectStrings.MatchTest1,
                },
            };
        }
    }
}
