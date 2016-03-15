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
    //function HelloService(__init)
    //{
    //    this.Hello = function(who)
    //    {
    //        var deferred = $q.defer();
    //        $http.post('/' + __init.__ID + '/Hello', {
    //            who: who,
    //        })
    //        .success(function(response)
    //        {
    //            deferred.resolve(new HelloModel({
    //                Greeting = response.Greeting,
    //                Times = response.Times,
    //                Goodbye = new GoodbyeService({
    //                    Name = response.Goodbye.Name,
    //                    __ID = response.Goodbye.__ID,
    //                }),
    //            }));
    //        }).failure(function(ex)
    //        {
    //            deferred.reject(ex);
    //        });
    //
    //        return deferred.promise;
    //    }
    //
    //    this.__ID = __init.__ID;
    // }
    //
    // function GoodbyeService(__init)
    // {
    //    this.Name = __init.Name;
    //
    //    this.Goodbye = function(what)
    //    {
    //        var deferred = $q.defer();
    //        $http.post('/' + __init.__ID + '/Goodbye',
    //        {
    //            what: what,
    //        })
    //        .success(function(response)
    //        {
    //            deferred.resolve(response);
    //        })
    //        .failure(function(ex)
    //        {
    //            deferred.reject(ex);
    //        });
    //
    //        return deferred.promise;
    //    }
    //
    //    this.__ID = __init.__ID;
    //}
}