using xs.concurrent;
using xs.server;

using Newtonsoft.Json.Linq;
using excess_ui.Interfaces;

namespace excess_ui.Project
{

	[route("/project/compile")]  
	function compile(string code)
	{
		inject
		{
			ICodeTranspiler _transpiler;
		}  

		return _transpiler.Transpile(code);
	} 

	[route("/project/load")]  
	function load(string projectName)
	{
		inject
		{
			IProjectRepository _repo;
		} 

		return _repo.Open(projectName);
	} 

	[route("/project/readFile")]  
	function readFile(int fileID)
	{
		inject
		{
			IProjectRepository _repo;
		} 

		return _repo.ReadFile(fileID);
	} 

	[route("/project/saveFile")]  
	function saveFile(int fileID, string content)
	{
		inject
		{
			IProjectRepository _repo;
		} 

		return _repo.WriteFile(fileID, content);
	} 

	[route("/project/deleteFile")]  
	function deleteFile(int fileID)
	{
		inject
		{
			IProjectRepository _repo;
		} 

		return _repo.DeleteFile(fileID);
	} 

	[route("/project/newFile")]  
	function newFile(int parentFileID, string fileName)
	{
		inject
		{
			IProjectRepository _repo;
		} 

		return _repo.NewFile(parentFileID, fileName);
	} 
}
