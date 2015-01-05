(function () {
    var controllerId = 'app.views.project';
    angular.module('app').controller(controllerId, [
        '$scope', '$rootScope', '$stateParams', 'dialogs', 'hotkeys', 'xsProject',
        function ($scope, $rootScope, $stateParams, dialogs, hotkeys, xsProject) {
            var vm = this;
            
            //project tree
            $scope.projectTree = null;

            //load project
            $scope.busy = true;
            $rootScope.$broadcast('loading-requests', 1);

            xsProject.loadProject($stateParams.projectId)
                .then(function(result) {
                    $scope.busy = false;
                    $scope.projectTree = result.data.tree;

                    loadFile(result.data.defaultFile);
                })
                .finally(function () {
                    $rootScope.$broadcast('request-loaded');
                });

            //file management
            var _fileCache = {};

            function fileLoaded(file, contents, inNewTab)
            {
                if (inNewTab)
                    addTab(file, contents);
                else
                    defaultTab(file, contents);
            }

            $scope.fileBusy = false;
            function loadFile(file, inNewTab)
            {
                var cached = _fileCache[file];
                if (cached)
                {
                    fileLoaded(file, cached.contents, inNewTab);
                    return;
                }

                if ($scope.fileBusy)
                    return;

                $scope.fileBusy = true;
                xsProject.loadFile(file)
                    .then(function (result) {
                        _fileCache[file] = 
                        {
                            changed:  false,
                            contents: result.data
                        };

                        fileLoaded(file, result.data, inNewTab)
                    })
                    .finally(function () {
                        $scope.fileBusy = false;
                    });
            }

            //actions
            function selectFile(file, inNewTab)
            {
                var found = false;
                angular.forEach($scope.editors, function (value, key) {
                    if (value.name == file)
                    {
                        found = true;
                        value.active = true;
                        return;
                    }
                });

                if (!found)
                    loadFile(file, inNewTab);
            }

            $scope.projectAction = function (action, data)
            {
                switch (action)
                {
                    case "select-file":
                    {
                        selectFile(data);
                        break;
                    }
                    case "open-tab":
                    {
                        selectFile(data, true);
                        break;
                    }
                    case "add-class":
                    {
                        var dlg = dialogs.create('/App/Main/dialogs/newClass.html',
                                                 'newClassCtrl',
                                                 null,
                                                 { size: "sm" });

                        dlg.result.then(function (name) {
                            xsProject.createClass(name)
                                .then(function () {
                                    var projectRoot = $scope.projectTree[0];
                                    projectRoot.children.push(
                                    {
                                        label   : name,
                                        icon    : "fa-code",
                                        action  : "select-file",
                                        data    : name,
                                        actions : 
                                        [
                                          { id : "remove-file", icon : "fa-times-circle-o"       },
                                          { id : "open-tab",    icon : "fa-arrow-circle-o-right" },
                                        ]
                                    });
                                });
                        });
                        break;
                    }
                }
            }
            
            //compiler interface
            $scope.compileProject = function () {
            };

            $scope.runProject = function () {
            };

            //keyboard shortcuts
            hotkeys.add({
                combo: 'ctrl+shift+b',
                description: 'Compile',
                callback: function () {
                    $scope.compileProject();
                }
            });

            hotkeys.add({
                combo: 'ctrl+f5',
                description: 'Run',
                callback: function () {
                    $scope.runProject();
                }
            });

            hotkeys.add({
                combo: 'ctrl+s',
                description: 'Save',
                callback: function () {
                    $scope.saveFiles();
                }
            });
            //tab management
            $scope.editors =
            [
                { id: 1, name: "_" },
            ];

            var _currentTab = null;
            $scope.tabSelected = function (selected) {
                if (_currentTab != selected) {
                    _currentTab = selected;

                    _ignoreChange = true;
                    $scope.sourceCode = selected.sourceCode;
                }
            }

            function addTab(name, contents)
            {
                var id = $scope.editors.length + 1;
                $scope.editors.push({
                    id:         id,
                    name:       name,
                    changed:    false,
                    sourceCode: contents,
                    active:     true
                });

                _ignoreChange = true;
            }

            function defaultTab(file, content)
            {
                _currentTab = $scope.editors[0];
                if (_currentTab.changed)
                    saveFiles(true);

                _currentTab.changed    = false;
                _currentTab.name       = file;
                _currentTab.active     = true;
                _currentTab.sourceCode = content;

                _ignoreChange = true;
                $scope.sourceCode = content;
            }

            //editor management
            $scope.updateCodeEditor = function ()
            {
                $scope.editorResized = !$scope.editorResized;
            }

            //change management
            var _ignoreChange = false;

            $scope.sourceModified = false;
            $scope.sourceChanged = function () {
                if (_ignoreChange)
                {
                    _ignoreChange = false;
                    return;
                }

                $scope.sourceModified = true;
                var sourceEditor = $('#code-editor').isolateScope();

                _fileCache[_currentTab.name].changed  = true;
                _fileCache[_currentTab.name].contents = sourceEditor.content();
            }

            $scope.saveFiles = function()
            {
                angular.forEach(_fileCache, function (value, key) {
                    if (value.changed) {
                        xsProject.saveFile(key, value.contents);
                        value.changed = false;
                    }
                });

                $scope.sourceModified = false;
            }
            
            //console
            $scope.console = {};

            //test
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
})();