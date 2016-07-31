using xs.server;
using xs.concurrent;

using demo_transpiler;

namespace Home
{ 
	function Transpile(string text)    
	{
		inject 
		{
			ITranspiler	_transpiler;
		}      

		return _transpiler.Process(text);         
	}

	function TranspileGraph(string text)
	{
		inject 
		{
			IGraphTranspiler _graphTranspiler;
		}      

		return _graphTranspiler.Process(text);      
	} 
}
