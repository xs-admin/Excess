
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



    this.__ID = 'adeea9a6-0e62-486b-9f4d-934dc3511b60';
}])


