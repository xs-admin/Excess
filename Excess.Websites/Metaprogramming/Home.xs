using xs.server;
using xs.concurrent;

using metaprogramming.interfaces;

namespace metaprogramming
{
	public service Home   
	{
		inject 
		{
			ICodeTranspiler		 _transpiler;
			IGraphTranspiler _graphTranspiler;  
		}   

		public string Transpile(string text)
		{
			return _transpiler.Transpile(text);     
		}

		public string TranspileGraph(string text) 
		{
			return _graphTranspiler.Transpile(text);         
		} 
	} 
}
