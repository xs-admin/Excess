using xs.server;					

namespace metaprogramming
{
	server Development
	{
		on port 1080
		static files @..\..\client\app
	}
}
