
            var xsServices = angular.module('xs.Services', []);
            
            xsServices.service('metaprogramming.Home', ['$http', '$q', function($http, $q)
            {
                
            this.Transpile = function (text)
            {
                var deferred = $q.defer();

                $http.post("/transpile/code", {
                    text : text,

                }).then(function(response) {
                    deferred.resolve(response);
                }, function(ex){
                    deferred.reject(ex);
                });

                return deferred.promise;
            }


            this.TranspileGraph = function (text)
            {
                var deferred = $q.defer();

                $http.post("/transpile/graph", {
                    text : text,

                }).then(function(response) {
                    deferred.resolve(response);
                }, function(ex){
                    deferred.reject(ex);
                });

                return deferred.promise;
            }



                this.__ID = 'a38b84c9-958a-41b5-a746-9716a12b3eb5';
            }])
