
            var xsServices = angular.module('xs.Services', []);
            
            xsServices.service('metaprogramming.Home', ['$http', '$q', function($http, $q)
            {
                
            this.Transpile = function (text)
            {
                var deferred = $q.defer();

                $http.post("/transpile/code", {
                    text : text,

                }).then(function(__response) {
                    deferred.resolve(__response.data.__res);
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

                }).then(function(__response) {
                    deferred.resolve(__response.data.__res);
                }, function(ex){
                    deferred.reject(ex);
                });

                return deferred.promise;
            }



                this.__ID = '5cb6feb9-10c4-4be0-a87d-b1e3e3ba0761';
            }])
