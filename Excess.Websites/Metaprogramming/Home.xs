using xs.server;
using xs.concurrent;

namespace metaprogramming
{
	public service Home
	{
		ITranspiler _transpiler;
		//constructor(ITranspiler transpiler)
		//{
		//	_transpiler = transpiler;
		//}   

		public string Transpile(string text)
		{
			return _transpiler.Process(text);   
		}
	}
}
