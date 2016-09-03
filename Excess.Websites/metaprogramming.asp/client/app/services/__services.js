
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



    this.__ID = 'a2186f60-d4e4-4e8c-a69e-0763e7697da0';
}])


