(function() {
    var controllerId = 'app.views.home';
    angular.module('app').controller(controllerId, [
        '$scope', '$rootScope', '$modal', '$state', 'xsCompiler',
        function ($scope, $rootScope, $modal, $state, xsCompiler) {
            var vm = this;
            
            $scope.sourceCode = "//write xs here";
            $scope.targetCode = "//result c# here after compiling";

            //loading
            $rootScope.$broadcast('loading-requests', 3 );

            //translation samples
            $scope.gotSamples = false;
            xsCompiler.samples()
                .then(function (result) {
                    $scope.samples = result.data;
                })
                .finally(function () {
                    $scope.gotSamples = true;
                    $rootScope.$broadcast('request-loaded');
                });

            $scope.selectSample = function (sampleId) {
                xsCompiler.sample(sampleId)
                    .then(function (result) {
                        $scope.sourceCode = result.data;
                    })
            }

            //project samples
            $scope.gotSampleProjects = false;
            xsCompiler.sampleProjects()
                .then(function (result) {

                    var projects = result.data; 
                    angular.forEach(projects, function (value, key) {
                        var icon = "fa-circle-o";
                        if (value.ProjectType == 'console') 
                            icon = 'fa-terminal';
                        else if (value.ProjectType == 'dsl') 
                            icon = 'fa-signal';

                        value.Icon = icon;
                    });

                    $scope.sampleProjects = projects;
                })
                .finally(function () {
                    $scope.gotSampleProjects = true;
                    $rootScope.$broadcast('request-loaded');
                });

            $scope.selectProject = function (projectId)
            {
                $state.go('project', { projectId: projectId });
            };

            //editor keywords
            $scope.editorKeywords = null;
            xsCompiler.keywords()
                .then(function (value) {
                    $scope.editorKeywords = value.data;
                })
                .finally(function () {
                    $rootScope.$broadcast('request-loaded');
                });

            //translate
            $scope.translateSource = function () {
                var sourceEditor = $('#source-editor').isolateScope();

                xsCompiler.translate(sourceEditor.content())
                    .then(function (result) {
                        $scope.targetCode = result.data;
                    })
                    .catch(function () {
                        alert("Error");
                    });
            }

            //code mirror must be resized manually
            $scope.resizeSource = function () {
                $scope.sourceResized = !$scope.sourceResized;
            }

            $scope.resizeTarget = function () {
                $scope.targetResized = !$scope.targetResized;
            }
        }
    ]);
})();