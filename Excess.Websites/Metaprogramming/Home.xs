using xs.server;
using xs.concurrent;

namespace metaprogramming
{
	public service Home  
	{
		inject 
		{
			ITranspiler _transpiler;
		}   

		public string Transpile(string text)
		{
			return _transpiler.Process(text);    
		}
	}
}
