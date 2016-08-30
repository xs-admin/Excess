using xs.server;
using xs.concurrent;

using metaprogramming.interfaces;

namespace metaprogramming.Home
{ 
	[route("/transpile/code")]
	function Transpile(string text)    
	{
		inject 
		{
			ICodeTranspiler	_transpiler;
		}       

		return _transpiler.Transpile(text);         
	}

	[route("/transpile/graph")] 
	function TranspileGraph(string text) 
	{
		inject 
		{
			IGraphTranspiler _graphTranspiler;
		}      

		return _graphTranspiler.Transpile(text);      
	}  
}
