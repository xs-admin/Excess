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
            xsCompiler.samples()
                .then(function (result) {
                    $scope.samples = result.data;
                })
                .finally(function () {
                    $rootScope.$broadcast('request-loaded');
                });

            $scope.selectedSample = null;
            $scope.$watch("selectedSample", function (value) {
                if (value)
                {
                    xsCompiler.sample(value.id)
                        .then(function (result) {
                            $scope.sourceCode = result.data;
                        })
                }
            });

            //project samples
            xsCompiler.sampleProjects()
                .then(function (result) {
                    $scope.sampleProjects = result.data;
                })
                .finally(function () {
                    $rootScope.$broadcast('request-loaded');
                });

            $scope.selectedProject = null;
            $scope.$watch("selectedProject", function (value) {
                if (value) {
                    $state.go('project', { projectId: value.ID });
                }
            });

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