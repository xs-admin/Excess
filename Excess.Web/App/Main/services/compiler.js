angular.module('app')

.service('xsCompiler', ['$http', function ($http) {

    this.translate = function (text)
    {
        return $http.get('/XS/Translate',
        {
            params: { text: text }
        });
    }

}]);