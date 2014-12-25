angular.module('app')

.controller("newProjectCtrl",
['$scope', 'xsProject', function ($scope, xsProject)
{
    //default values
    $scope.projectName = "Enter Name";
    $scope.projectKind = "dsl";

    $scope.dslConfiguration =
    {
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
}]);
