﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Excess.Web.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ProjectTemplates {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ProjectTemplates() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Excess.Web.Resources.ProjectTemplates", typeof(ProjectTemplates).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to class application
        ///{
        ///    void main()
        ///    {
        ///        //your code here
        ///    }
        ///}.
        /// </summary>
        internal static string ConsoleApplication {
            get {
                return ResourceManager.GetString("ConsoleApplication", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public class Linker : ManagedLinker
        ///{
        ///    public SyntaxNode Link(SyntaxNode node, SemanticModel model)
        ///    {
        ///        //global changes here
        ///        return node;
        ///    }
        ///}.
        /// </summary>
        internal static string DSLLinker {
            get {
                return ResourceManager.GetString("DSLLinker", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     public SyntaxNode ParseCodeHeader(SyntaxNode node)
        ///    {
        ///        return null;
        ///    }
        ///
        ///    public SyntaxNode ParseCode(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code, bool expectsResult)
        ///    {
        ///        throw new NotImplementedException();
        ///    }.
        /// </summary>
        internal static string DSLParseCode {
            get {
                return ResourceManager.GetString("DSLParseCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     public SyntaxNode ParseMethod(SyntaxNode node, string id, ParameterListSyntax args, BlockSyntax code)
        ///    {
        ///        throw new NotImplementedException();
        ///    }.
        /// </summary>
        internal static string DSLParseMember {
            get {
                return ResourceManager.GetString("DSLParseMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     public SyntaxNode ParseNamespace(SyntaxNode node, string  id, ParameterListSyntax args, BlockSyntax code)
        ///    {
        ///        throw new NotImplementedException();
        ///    }
        ///.
        /// </summary>
        internal static string DSLParseNamespace {
            get {
                return ResourceManager.GetString("DSLParseNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public class Parser : ManagedParser&lt;Linker&gt;
        ///{{
        ///{0}
        ///}}.
        /// </summary>
        internal static string DSLParser {
            get {
                return ResourceManager.GetString("DSLParser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     public SyntaxNode ParseClass(SyntaxNode node, string  id, ParameterListSyntax args)
        ///    {
        ///        throw new NotImplementedException();
        ///    }
        ///.
        /// </summary>
        internal static string DSLParseType {
            get {
                return ResourceManager.GetString("DSLParseType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using Excess.Compiler.Core;
        ///public class ExtensionPlugin
        ///{
        ///    public static ICompilerInjector&lt;SyntaxToken, SyntaxNode, SemanticModel&gt; Create()
        ///    {
        ///        return new DelegateInjector&lt;SyntaxToken, SyntaxNode, SemanticModel&gt;(compiler =&gt; Extension.Apply(compiler));
        ///    }
        ///}.
        /// </summary>
        internal static string ExtensionPlugin {
            get {
                return ResourceManager.GetString("ExtensionPlugin", resourceCulture);
            }
        }
    }
}
