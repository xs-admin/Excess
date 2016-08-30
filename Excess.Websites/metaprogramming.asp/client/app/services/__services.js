
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



    this.__ID = '41b27749-1ee4-4447-90e3-cc3a8bb0fecb';
}])


