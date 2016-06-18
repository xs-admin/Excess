
            var xsServices = angular.module('xs.Services', []);
            
            xsServices.service('Home', ['$http', '$q', function($http, $q)
            {
                
            this.Transpile = function (text)
            {
                var deferred = $q.defer();

                $http.post('/Home' + '/Transpile', {
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

                $http.post('/Home' + '/TranspileGraph', {
                    text : text,

                }).then(function(response) {
                    deferred.resolve(response);
                }, function(ex){
                    deferred.reject(ex);
                });

                return deferred.promise;
            }



                this.__ID = '9a54e6fb-6f9c-49ba-b698-c9318adddddd';
            }])
