(function () {
    var controllerId = 'app.views.project';
    angular.module('app').controller(controllerId, [
        '$scope', '$stateParams', function ($scope, $stateParams) {
            var vm = this;
            
            alert($stateParams.projectId);

            var _currTest = 0;
            $scope.projectAction = function (action, data)
            {
                switch (action)
                {
                    case "new_tab":
                    {
                        _currTest++;
                        addEditor("testing" + _currTest);
                        break;
                    }

                    case "select-file":
                    {
                        $scope.sourceCode = data.file;
                        break;
                    }

                    case "test-console":
                    {
                        $scope.console.add('Hello, dolly');
                        break;
                    }
                }
            }

            function addEditor(name)
            {
                var id = $scope.editors.length + 1;
                $scope.editors.push({
                    id:   id,
                    name: name
                });

                _editors[name] = {
                    changed: false,
                    sourceCode: 'Tab' + id
                };
            }

            $scope.updateCodeEditor = function ()
            {
                $scope.editorResized = !$scope.editorResized;
            }

            var _editors       = {};
            var _currentEditor = null;

            $scope.editorSelected = function (editor)
            {
                if (_currentEditor != editor)
                {
                    var selected = _editors[editor];
                    if (selected)
                    {
                        _currentEditor = editor;
                        $scope.sourceCode = selected.sourceCode;
                    }
                }
            }

            $scope.sourceModified = false;
            $scope.sourceChanged = function () {
                $scope.sourceModified = true;
            }
            
            //console
            $scope.console = {};

            //data
            $scope.editors =
            [
                { id: 1, name: "application" },
            ];

            $scope.testTree =
            [
                {
                    label: 'project',
                    icon: 'fa-star',
                    children:
                    [
                        {
                            label: 'folder1',
                            icon: 'fa-folder',
                            children:
                            [
                                {
                                    label: 'file1',
                                    action: "select-file",
                                    data: {
                                        file: "filename1"
                                    }
                                },
                                {
                                    label: 'file2',
                                    action: "test-console",
                                },
                                { label: 'file3' },
                            ]
                        },

                        { label: 'file4', icon: 'fa-file' },
                        {
                            label: 'file5',
                            icon: 'fa-file',
                            actions: 
                            [
                                { id: 'new_tab', icon: 'fa-star'},

                            ]
                        },
                    ]
                }
            ];
        }
    ])

    .controller('TabsChildController', function () {
    });
})();