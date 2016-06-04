using xs.ninject;

namespace metaprogramming.server
{
	injector  
	{
		ITranspiler = Transpiler;   
		IGraphTranspiler = GraphTranspiler;
	}
}
