angular.module('app')

.service('xsCompiler', ['$http', function ($http) {

    this.translate = function (text)
    {
        return $http.get('/XS/Translate',
        {
            params: { text: text }
        });
    }

    this.samples = function () {
        return $http.get('/XS/GetSamples');
    }

    this.sample = function (id) {
        return $http.get('/XS/GetSample',
        {
            params: { id: id }
        });
    }

    this.keywords = function () {
        return $http.get('/XS/GetKeywords');
    }

    this.sampleProjects = function () {
        return $http.get('/XS/GetSampleProjects');
    }
}]);