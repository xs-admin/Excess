using xs.ninject;

using demo_transpiler;

namespace metaprogramming.server
{
	injector     
	{
		ITranspiler = Transpiler;   
		IGraphTranspiler = GraphTranspiler; 
	}
}
