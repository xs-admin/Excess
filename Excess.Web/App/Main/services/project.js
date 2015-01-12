angular.module('app')

.service('xsProject', ['$http', '$timeout', function ($http, $timeout) {

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
        return $http.post('/Project/SaveFile', {
            file: file,
            contents: contents
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

    this.createProject = function (projectName, projectType, projectData) {
        return $http.get('/Project/CreateProject', {
            params:
            {
                projectType: projectType,
                projectName: projectName,
                projectData: projectData
            }
        });
    }
    

    function getNotifications(notify) {

        function tick() {
            $http.get('/Project/Notifications')
                .then(function (result) {
                    var finished = false;

                    angular.forEach(result.data, function (value, key) {
                        notify(value);

                        if (value.Kind == 4) {
                            finished = true;
                        }
                    });

                    if (!finished)
                        $timeout(getNotifications, 100);
                })
        }

        $timeout(tick, 100);
    }

    this.userProjects = function () {
        return $http.get('/Project/UserProjects');
    }

    this.compile = function (notify) {
        getNotifications(notify);
        return $http.post('/Project/Compile');
    }

    this.execute = function (notify) {
        getNotifications(notify);
        return $http.post('/Project/Execute');
    }

    this.debugDSL = function (text) {
        return $http.get('/Project/debugDSL', {
            params: { text: text }
        });
    }
}]);