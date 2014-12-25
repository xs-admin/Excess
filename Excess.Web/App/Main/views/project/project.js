(function () {
    var controllerId = 'app.views.project';
    angular.module('app').controller(controllerId, [
        '$scope', function ($scope) {
            var vm = this;
            
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
                                { label: 'file2' },
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