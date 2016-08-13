using Excess.Compiler.Mock;
using Excess.Runtime;
using SQL.Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using System.Data.SQLite;
using xslang;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Data;

namespace Tests.Mock
{
    public class DapperMock
    {
        static string ConnectionString = "FullUri=file::memory:?cache=shared";

        public static DapperMock Compile(string text, string database)
        {
            var assembly = ExcessMock.Build(text,
                builder: compiler =>
                {
                    Functions.Apply(compiler);
                    DapperExtension.Apply(compiler);
                },
                referenceTypes: new[] {
                    typeof(Dapper.SqlMapper),
                    typeof(__Scope),
                    typeof(IDbConnection),
                    typeof(IEnumerable<>),
                    typeof(SqlConnection),
                    typeof(SQLiteConnection),
                    typeof(Component),
                });


            if (assembly == null)
                throw new InvalidOperationException();

            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            if (database != null)
            {
                connection.Execute(database);
            }

            return new DapperMock(assembly, connection);
        }

        Dictionary<string, Func<object>> _functions = new Dictionary<string, Func<object>>();
        private DapperMock(Assembly assembly, SQLiteConnection connection)
        {
            var container = assembly
                .GetTypes()
                .Single(type => type.Name == "Functions");

            var methods = container
                .GetMethods()
                .Where(method => method.IsStatic 
                              && method.IsPublic
                              && method.GetParameters().Length == 1);

            foreach (var method in methods)
            {
                _functions[method.Name] = () =>
                {
                    var scope = new __Scope(null);
                    scope.set<IDbConnection>(connection);
                    return method.Invoke(null, new object[] { scope });
                };
            }
        }

        public IEnumerable GetMany(string method)
        {
            return (IEnumerable)_functions[method]();
        }

        public IEnumerable<T> GetMany<T>(string method)
        {
            return (IEnumerable<T>)_functions[method]();
        }

        public object GetOne(string method)
        {
            return _functions[method]();
        }
    }
}
