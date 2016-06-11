using xs.ninject;

namespace metaprogramming_asp.server  
{
	injector    
	{
		ITranspiler = Transpiler;   
		IGraphTranspiler = GraphTranspiler;
	}
}
