angular.module('app')

.service('xsProject', ['$http', function ($http) {

    this.loadProject = function (id) {
        return $http.get('/Project/LoadProject',
        {
            params: { projectId: id }
        });
    }

    this.loadFile = function (file) {
        return $http.get('/Project/LoadFile',
        {
            params: { file: file }
        });
    }

    this.saveFile = function (file, contents) {
        return $http.get('/Project/SaveFile', {
            params: 
            {
                file: file,
                contents: contents
            }
        });
    }

    this.createClass = function (className) {
        return $http.get('/Project/CreateClass', {
            params:
            {
                className: className
            }
        });
    }
}]);