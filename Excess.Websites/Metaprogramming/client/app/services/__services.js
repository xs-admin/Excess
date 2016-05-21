
            var xsServices = angular.module('xs.Services', []);
            
            xsServices.service('Home', ['$http', '$q', function($htpp, $q)
            {
                
            this.Transpile = function (text)
            {
                var deferred = $q.defer();

                $http.post('/' + __init.__ID + '/Transpile', 
                {
                    text : text,

                })
                .success(function(response)
                {
                    deferred.resolve(response);
                })
                .failure(function(ex)
                {
                    deferred.reject(ex);
                });

                return deferred.promise;
            }


                this.__ID = '312d9508-889d-4c0d-afb6-af96e2096cfd'
            }])
