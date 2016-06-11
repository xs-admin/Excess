
            var xsServices = angular.module('xs.Services', []);
            
            xsServices.service('Home', ['$http', '$q', function($http, $q)
            {
                
            this.Transpile = function (text)
            {
                var deferred = $q.defer();

                $http.post('/' + this.__ID + '/Transpile', {
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

                $http.post('/' + this.__ID + '/TranspileGraph', {
                    text : text,

                }).then(function(response) {
                    deferred.resolve(response);
                }, function(ex){
                    deferred.reject(ex);
                });

                return deferred.promise;
            }


                this.__ID = 'b1f32524-0402-4721-9492-c7db004fb252';
            }])
