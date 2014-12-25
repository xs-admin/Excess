angular.module('app')

.service('xsCompiler', ['$http', function ($http) {

    this.translate = function (text)
    {
        return $http.get('/compiler/translate',
        {
            params: { text: text }
        });
    }

}]);