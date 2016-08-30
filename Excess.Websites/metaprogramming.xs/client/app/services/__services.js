
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


    this.__ID = 'c3cf038c-40df-452d-85e4-ce45e95910c3';
}])


