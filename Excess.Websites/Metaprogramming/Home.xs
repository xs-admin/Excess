using xs.server;
using xs.concurrent;

namespace metaprogramming
{
	public service Home  
	{
		inject 
		{
			ITranspiler		 _transpiler;
			IGraphTranspiler _graphTranspiler;
		}   

		public string Transpile(string text)
		{
			return _transpiler.Process(text);    
		}

		public string TranspileGraph(string text)
		{
			return _graphTranspiler.Process(text);    
		}
	} 
}
