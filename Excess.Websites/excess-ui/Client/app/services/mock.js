angular.module('mock.project', [])
    .service('$project', ['$q', function ($q) {
        
        this.tree = function () {
            return [];
        };

        this.file = function (id) {
            return "";
        };

        this.compile = function (listener) {
        };
    }]);
