# Excess

Excess is a metaprogramming platform intended to grow programming languages organically. 
We take existing, stable languages (currently C#) and let the community extend them,
as opposed to wait years for minimal version-to-version improvement from the owners.
All while retaining our biggest advantage:

> Other than extensions, you only writes code in a familiar, proven language. 

Excess is a _transpiler_, meaning the programming language extensions are converted into the source language
and reuse it as much as possible. The advantages are clear: reuse the host language's libraries, code generation, etc.
Furthermore, you can use the host compiler as well: never parse expressions or basic statements again. 

As an extension writer, all the heavy lifting will be done for you, including semantical information:
You can query the types of your simbols once compiled and keep transforming based on that information.

However, you need not be an advanced programming language developer to use Excess, 
you can simply use community-made extensions: 

## Getting started as a user

Our transpilation process creates source code in C#. So, at a minimum, you can use our [CLI](http://download_cli.com)  
and do whatever you wish with the generated cs file. However, the platform is much more powerful when used semantically.
As such, the CLI is also able to compile whole solutions. But CLIs are so _linuxy_...
 
The easy way to use Excess is, thus, our [Visual Studio Extension] (http://download_vsix.com), which is used as follows:

1. Add a .xs file to your project.
2. Install extensions as Nuget packages.
3. Include the extensions you need with **using** _xs.<extension name>_

### Resources

Other than the non-existent community extensions, we provide you with the following resources:

- A [mini-language called xs](http://doc.xs) is available by default in .xs files.
- A set of [in-house developed extensions](http://doc.extensions) which can be installed from [this nuget package](http://nuget.extensions).
- Our products:
  - **xs.concurrent** for concurrent programming, install from [nuget](http://nuget.concurrent).
  - **xs.server** for distributed programming, including web apps, install from [nuget](http://nuget.concurrent).

## Getting started as an extension writer

Now for the real fun part, where we take the language and we make it ours, for the general user
or for particular domains. Let us count thw ways you may do so:

1. **Low level lexical extensions**, such as js-style [arrays](http://code.arrays),
   where your extension changes the source before the C# compiler processes it.
2. **Syntax extensions**, such as [contracts](http://code.contracts), where you transform
   existing language statements to fit your needs.
3. **Member extensions**, such as [constructors](http://code.constructors), where you are 
   able to define special items.
4. **Type extensions**, such as the [server configurator](http://code.server), where types
   are constrained to a small set of operations.
5. **Complex super-object type modifiers**, such as [concurrent](http://code.server), where 
   types are transformed (constrained or expanded, or both).
6. **Traditional grammars**, such as [json](http://code.json) or [R](http://code.r), where 
   you embed complete languages at any level or you tree (as types, members or code).
7. **A brand new type of grammar**: the indentation grammar. As used in our [setting sample](http://code.settings),
   for python-like languages.
8. **Compilation-wide extensions**, such as the [angular services](http://code.angular services)    
   where modification to your project are performed after compilation. In this instance, we generated
   java-script files and add it to the solution.

The documentation for most these features is either in flux or non-existing at the moment, 
so we recommend the source code as the best way to learn. In general, the formula is always the same.
Extensions will register functions to match patterns at various stages of the pipeline. 
Transformation functions will be scheduled for when the match functions are succesful.
Changes to the source will kept been scheduled until the process is finished 
and the remaining C# code represents the intention of the extension maker. 

###The pipeline
Our compiler follows the traditional compiler pipeline described bellow.
In other words, it is [multi-pass](http://fift.element).
On each pass, more information is available to the extension:

- **Lexical Pass:** Nothing has compiled, the extension writer can alter token patterns 
	and, if neccessary, schedule further transform down the pipeline.
 
- **Syntax Pass:** A valid native AST is produced. In our current implementation, a [Roslyn](http://roslyn) SyntaxTree. 
	From this point forward, the extension writer will apply node to node transformation, which is simpler. 
	Examples of this transformations include: 
	- adding/removing parameters, members, statements or types. 
	- replacing nodes altogether (like case statements in a switch for if/elses).
	- instantiate templates, most likely invoking state of the art libraries.

- **Semantical Pass:** All tress in the current compilation (say, solution) have been "linked". 
	As such, the extension maker can match nodes based on type, find references, etc. 
	This pass will not finish until no more changes are scheduled. 
	Examples of such transformation include finding the type for a [method](http://code.method)    

- **Compilation Pass:** We are ready now to emit code, but the extension maker can still apply 
steps such as adding or modifying compilation items. Also, at this point, the compilation is 
"final". 

Enjoy.