(function() {
    var controllerId = 'app.views.home';
    angular.module('app').controller(controllerId, [
        '$scope', '$modal', 'xsCompiler', function ($scope, $modal, xsCompiler) {
            var vm = this;
            
            $scope.sourceCode = "";
            $scope.targetCode = "";

            $scope.sampleStatus = 'Getting Samples...';
            xsCompiler.samples()
                .then(function (result) {
                    $scope.sampleStatus = 'Compile a sample';
                    $scope.samples = result.data;
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

            $scope.editorKeywords = null;
            xsCompiler.keywords()
                .then(function (value) {
                    $scope.editorKeywords = value.data;
                });

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

            $scope.newProject = function () {
                var modalInstance = $modal.open({
                    templateUrl: '/App/Main/dialogs/newProject.html',
                    controller: $scope.newProjectCtrl,
                    windowClass: "app-modal-window",
                    backdrop: true,
                    resolve: {
                    }
                });

                modalInstance.result.then(function(){
                }, function(){
                });
            }


            //code mirror must be resized manually
            $scope.resizeSource = function () {
                $scope.sourceResized = !$scope.sourceResized;
            }

            $scope.resizeTarget = function () {
                $scope.targetResized = !$scope.targetResized;
            }

            //code mirror has its own instances, which we need to cache
            var _sourceEditor, _targetEditor;

            function getEditors() {
                if (_sourceEditor && _targetEditor)
                    return;

                _sourceEditor = getCodeMirror('#source-editor');
                _targetEditor = getCodeMirror('#target-editor');
            }

            function getCodeMirror(elem)
            {
                var target = $(elem).isolateScope();
                if (!target || !target.instance)
                    throw "Element " + elem + " is not a code mirror directive";

                return target.instance;
            }
        }
    ]);
})();