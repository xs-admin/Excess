using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.DSL
{
    class distributed
    {
        public SyntaxNode ParseClass(SyntaxNode node, string id, ParameterListSyntax args)
        {
            ClassDeclarationSyntax decl = (ClassDeclarationSyntax)node;

            var found_ctor = false;
            foreach (var member in decl.Members)
            {
                if (member is ConstructorDeclarationSyntax)
                {
                    ConstructorDeclarationSyntax ctor = (ConstructorDeclarationSyntax)member;
                    if (found_ctor)
                    {
                        //error
                        break;
                    }

                    found_ctor = true;
                    _plan.EntryPoint(ctor);
                }
                else if (member is MethodDeclarationSyntax)
                {
                    _plan.AddConsumer((MethodDeclarationSyntax)member);
                }
                else 
                {
                    //error
                }
            }

            return _plan.Resolve();
        }

        execution_plan _plan;
    }

    class execution_plan
    {
        private Dictionary<TypeSyntax, string> _producers = new Dictionary<TypeSyntax, string>();
        private List<MethodDeclarationSyntax>  _consumers = new List<MethodDeclarationSyntax>();
        private ConstructorDeclarationSyntax   _entry;

        internal void AddConsumer(MethodDeclarationSyntax member)
        {
            _consumers.Add(member);
        }

        internal void addProducer(string name, TypeSyntax type)
        {
            _producers[type] = name;
        }

        internal SyntaxNode Resolve()
        {
            List<StatementSyntax> execution = new List<StatementSyntax>();
            while (true)
            {
                int consumerCount = _consumers.Count;

                List<MethodDeclarationSyntax> pending = new List<MethodDeclarationSyntax>();
                foreach (var consumer in _consumers)
                {
                    bool match = true;
                    List<string> args = new List<string>();

                    foreach (var param in consumer.ParameterList.Parameters)
                    {
                        if (!_producers.ContainsKey(param.Type))
                        {
                            match = false;
                            args.Add(_producers[param.Type]);
                            break;
                        }
                    }

                    if (match)
                    {
                        StatementSyntax exec;
                        if (consumer.ReturnType.ToString() != "void")
                            exec = SyntaxFactory.
                    }
                    else
                        pending.Add(consumer);
                }

                if (pending.Count == consumerCount)
                {
                    //error
                    break;
                }
            }
        }

        internal void EntryPoint(ConstructorDeclarationSyntax ctor)
        {
            _entry = ctor;
            foreach (var param in ctor.ParameterList.Parameters)
            {
                addProducer(param.Identifier.ToString(), param.Type);
            }
        }
    }
}
