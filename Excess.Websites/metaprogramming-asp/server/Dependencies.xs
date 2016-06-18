using xs.ninject;
using demo_transpiler;

namespace metaprogramming_asp.server  
{
	injector     
	{
		ITranspiler = Transpiler;   
		IGraphTranspiler = GraphTranspiler;
	}
}
