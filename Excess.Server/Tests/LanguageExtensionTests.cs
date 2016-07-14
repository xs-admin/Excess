using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Server.Compiler;

namespace Tests
{
    [TestClass]
    public class LanguageExtensionTests
    {
        [TestMethod]
        public void Server_Usage()
        {
            //setup
            const string Code = @"
            namespace SomeNS            
            {
                server SomeServer()
                {
                    @http://*.1080
                    identity @tcp://*.10800 

                    new instance 
                        on port 2080
                        hosting SomeService
                }
            }";

            string output;
            var tree = Mock.Compile(Code, out output);

            //should have 2 classes marked as [ServerConfiguration]
            Assert.AreEqual(2, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(@class => @class
                    .AttributeLists
                    .Any(attrList => attrList
                        .Attributes
                        .Any(attr => attr.Name.ToString() == "ServerConfiguration")))
                .Count());

            //both should have a "start" method 
            Assert.AreEqual(2, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "Start")
                .Count());
        }

        [TestMethod]
        public void Service_Usage()
        {
            //setup
            const string Code = @"
            namespace SomeNS            
            {
                public service SomeService
                {
                    public void someMethod()
                    {
                    }
                }
            }";

            string output;
            var tree = Mock.Compile(Code, out output);

            //should have a class marked as [Service]
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(@class => @class
                    .AttributeLists
                    .Any(attrList => attrList
                        .Attributes
                        .Any(attr => attr.Name.ToString() == "Service")))
                .Count());

            //should have inherit ConcurrentObject
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(method => method
                    .BaseList
                    .Types
                    .Any(type => type.Type.ToString() == "ConcurrentObject"))
                .Count());
        }

        [TestMethod]
        public void Services_ShouldGenerateAngularServices()
        {
            //setup
            const string Code = @"
            namespace SomeNS            
            {
                public service SomeService
                {
                    public void someMethod()
                    {
                    }
                }
            }";

            var errorList = new List<string>();
            var compilation = Mock.Build(Code, errors: errorList, generateJSFiles: true);

            Assert.IsFalse(errorList.Any());

            var serverConfig = compilation.Scope.get<IServerConfiguration>();
            Assert.IsNotNull(serverConfig);

            var clientCode = serverConfig.GetClientInterface();

            //should generate an angular service
            Assert.IsTrue(clientCode.Contains("xsServices.service('SomeService', ['$http', '$q', function($http, $q)"));

            //should generate internal methods
            Assert.IsTrue(clientCode.Contains("this.someMethod = function ()"));

            //should generate the post method
            Assert.IsTrue(clientCode.Contains("$http.post('/' + this.__ID + '/someMethod'"));
        }

        [TestMethod]
        public void ConcurrentClasses_ShouldGenerateJavaScriptClasses()
        {
            //setup
            const string Code = @"
            concurrent class someConcurrentClass
            {
                public string Hello(string what)
                {
                    return ""Hello "" + what;
                }
            }

            concurrent class otherConcurrentClass
            {
                public string Goodbye(string what)
                {
                    return ""Goodbye "" + what;
                }
            }";

            var errorList = new List<string>();
            var compilation = Mock.Build(Code, errors: errorList, generateJSFiles: true);
            
            Assert.IsFalse(errorList.Any());

            var serverConfig = compilation.Scope.get<IServerConfiguration>();
            Assert.IsNotNull(serverConfig);

            var clientCode = serverConfig.GetClientInterface();

            //should generate a constructor for the clases
            Assert.IsTrue(clientCode.Contains("someConcurrentClass = function (__ID)")
                && clientCode.Contains("otherConcurrentClass = function (__ID)"));

            //should internal methods
            Assert.IsTrue(clientCode.Contains("this.Hello = function (what)")
                && clientCode.Contains("this.Goodbye = function (what)"));
        }

        [TestMethod]
        public void Functional_ShouldGenerateAngularServices()
        {
            //setup
            const string Code = @"
            namespace Some.Namespace
            {
                public function SomeFunction(string what)
                {
                    return ""Hello "" + what;
                }

                public function SomeOtherFunction(string what)
                {
                    return ""Other "" + what;
                }
            }";

            var errorList = new List<string>();
            var compilation = Mock.Build(Code, errors: errorList, generateJSFiles: true);

            Assert.IsFalse(errorList.Any());

            var serverConfig = compilation.Scope.get<IServerConfiguration>();
            Assert.IsNotNull(serverConfig);

            var clientCode = serverConfig.GetClientInterface();

            //should generate an angular service naed as the namespace
            Assert.IsTrue(clientCode.Contains("xsServices.service('Some.Namespace', ['$http', '$q', function($http, $q)"));

            //should generate a post to /Some/Namespace/SomeFunction
            Assert.IsTrue(clientCode.Contains("$http.post('/Some/Namespace' + '/SomeFunction'"));
        }

        [TestMethod]
        public void Debug()
        {
            string text;
            Mock.Compile(@"
            public service TestService
            {
                public string Hello(string what)
                {
                    return ""Hello "" + what;
                }
            }", out text);

            Assert.IsNotNull(text);
        }
    }
}
