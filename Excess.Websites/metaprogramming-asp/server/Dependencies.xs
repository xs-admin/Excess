using xs.ninject;
using metaprogramming.interfaces;
using metaprogramming.server.Roslyn;

namespace metaprogramming.server  
{
	injector     
	{
		ICodeTranspiler = Transpiler;   
		IGraphTranspiler = GraphTranspiler;      
	}
}
