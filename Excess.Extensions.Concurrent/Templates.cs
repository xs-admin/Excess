using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal static class Templates
    {
        public static Template EmptyMain = Template.Parse(@"
            void main()
            {
                continue;
            }");

        public static Template GetterProperty = Template.Parse(@"
            public Task<__0> _1()
            {
                return Node.send(this, _2);
            }");

        public static ExpressionSyntax MainContinuation = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    response.success(value);
                
                Node.finished(this);
            }");

        public static ExpressionSyntax MainContinuated = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    throw new InvalidOperationException(""infinite"");
            }");

        public static ExpressionSyntax SignalContinuation = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    response.success(value);
            }");

        public static Template EmptyPrivateMethod = Template.Parse(
          @"private void _0()
            {
                enqueue(__1, (Processor)null);
            }");

        public static Template PrivateSignal = Template.ParseStatement("{throw new InvalidOperationException(\"calling a private signal\");}");
    }
}
