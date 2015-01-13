angular.module('app')

.controller("newProjectCtrl",
['$scope', '$modalInstance', 'xsProject',
function ($scope, $modalInstance, xsProject)
{
    //default values
    $scope.projectName = "";
    $scope.projectKind = "console";
    $scope.finishText  = "Finish"; 

    $scope.dslConfiguration =
    {
        name:              "",
        parser:            "roslyn",
        linker:            "roslyn",
        extendsNamespaces: false,
        extendsTypes:      false,
        extendsMembers:    false,
        extendsCode:       false,
    };

    $scope.consoleConfiguration =
    {
        generateLibrary: false,
        onlyExe: true,
    };

    $scope.done = function()
    {
        $scope.finishText = "Creating Project...";

        var projectData = $scope.consoleConfiguration;
        if ($scope.projectKind == "dsl")
            projectData = $scope.dslConfiguration;

        xsProject.createProject($scope.projectName, $scope.projectKind, projectData)
            .then(function (result) {
                $modalInstance.close(result.data.projectId);
            });
    };
}]);
