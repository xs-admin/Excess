
var xsServices = angular.module('xs.Services', []);

xsServices.service('Home', ['$http', '$q', function($http, $q)
{
	
this.Transpile = function (text)
{
	var deferred = $q.defer();

    $http.post('/' + this.__ID + '/Transpile', {
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

    $http.post('/' + this.__ID + '/TranspileGraph', {
		text : text,

	}).then(function(__response) {
		deferred.resolve(__response.data.__res);
	}, function(ex){
		deferred.reject(ex);
    });

    return deferred.promise;
}


    this.__ID = '3d196d5a-03b2-4501-94be-04f87c15d2d1';
}])


