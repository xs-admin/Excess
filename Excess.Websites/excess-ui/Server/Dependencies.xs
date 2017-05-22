using xs.ninject;
using excess_ui.Interfaces;
using excess_ui.server.WebTranspilers;

namespace excess_ui
{
	injector 
	{
		//add your bindings like: Interface = Concrete;    
		ICodeTranspiler = CodeTranspiler;
		//IGraphTranspiler = GraphTranspiler; 
		//IFileRepository = FileRepository;
		IProjectRepository  = ProjectRepository ;
	}
}
