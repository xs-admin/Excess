using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Core;

namespace Excess.Compiler.GraphInstances
{
    public class Step
    {
        public Step(string owner)
        {
            Owner = owner;
            Before = new List<StatementSyntax>();
            After = new List<StatementSyntax>();
            Execute = new List<StatementSyntax>();
            Accessors = new Dictionary<string, string>();
            Inner = new Dictionary<string, StepContainer>();
            Types = new Dictionary<string, ClassDeclarationSyntax>();
        }

        public string Owner { get; set; }
        public Dictionary<string, string> Accessors { get; set; }

        public ICollection<StatementSyntax> Before { get; set; }
        public ICollection<StatementSyntax> Execute { get; set; }
        public ICollection<StatementSyntax> After { get; set; }
        public IDictionary<string, StepContainer> Inner { get; set; }
        public IDictionary<string, ClassDeclarationSyntax> Types { get; set; }

        public ClassDeclarationSyntax RegisteredType(string id, Template factory)
        {
            var result = null as ClassDeclarationSyntax;
            if (Types.TryGetValue(id, out result))
                return result;

            result = factory.Get<ClassDeclarationSyntax>(id);
            Types[id] = result;
            return result;
        }

        public ClassDeclarationSyntax GetType(string id)
        {
            var result = null as ClassDeclarationSyntax;
            if (Types.TryGetValue(id, out result))
                return result;
            return null;
        }

        public void SetType(string id, ClassDeclarationSyntax type)
        {
            Types[id] = type;
        }
    }

    public class StepContainer
    {
        public StepContainer(IEnumerable<Step> steps)
        {
            Steps = steps;
        }

        public IEnumerable<Step> Steps { get; }
    }
}
