using Excess.Concurrent.Runtime;
using Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeNS
{

function HelloService
{
    this.Hello = function (who)
    {
        var deferred = $q.defer();

        $http.post('/e4f3a060-e014-497f-8811-ad13358154fc/Hello',
        {
            who: who,
        })
        .success(function(response))
        {
            deferred.resolve(new HelloModel(
            {
                Greeting = response.Greeting,
                Times = response.Times,
                Goodbye = new GoodbyeService({
                    Name = response.Goodbye.Name,
            });,
        }););
        })
                .failure(function(ex))
                {
            deferred.reject(ex);
        });

                return deferred.promise;
    }

}

function GoodbyeService
{
                
            this.Name = "GoodbyeService";

            this.Goodbye = function (what)
            {
        var deferred = $q.defer();

                $http.post('/9421b080-b3b0-41a2-b4c0-c05374468407/Goodbye',
                {
        what: what,

                })
                .success(function(response))
                {
            deferred.resolve(response);
        })
                .failure(function(ex))
                {
            deferred.reject(ex);
        });

        return deferred.promise;
    }

}

}