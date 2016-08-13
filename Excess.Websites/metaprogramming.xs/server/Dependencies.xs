using xs.ninject;
using metaprogramming.interfaces;
using metaprogramming.server.WebTranspilers;

namespace metaprogramming.server
{
	injector   
	{
		ICodeTranspiler = CodeTranspiler;
		IGraphTranspiler = GraphTranspiler; 
	}
}
