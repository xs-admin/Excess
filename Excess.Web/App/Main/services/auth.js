angular.module('app')

.service('xsAuthentication', ['$http', function ($http) {

    var _session = null;

    this.createSession = function () {
        return $http.get('/Account/CurrentUser')
                    .then(function (result) {
                        if (result.data.UserId)
                            _session = result.data;
                        else
                            _session = null;
                    });
    }

    this.closeSession = function () {
        return $http.post('/Account/LogOff')
                    .then(function () {
                        _session = null;
                    });
    }

    this.getSession = function () {
        return _session;
    }

    this.isAuthenticated = function () {
        return !!_session;
    }
}])

.run(['$rootScope', 'xsAuthentication', function ($rootScope, xsAuthentication) {

    $rootScope.session = null;

    $rootScope.refreshSession = function () {
        xsAuthentication.createSession()
            .then(function () {
                $rootScope.session = xsAuthentication.getSession();
            });
    };

    $rootScope.refreshSession();
}]);