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
            {
                context.Samples.AddOrUpdate(sample);
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
    }
}
