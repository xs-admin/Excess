angular.module('excess.ui')
    .controller('xsuiIdeController', ['$scope', '$project', function ($scope, $project) {
        var vm = this;

        //status
        $scope.busy = true;
        $scope.fileBusy = false;
        $scope.compilerBusy = false;

        //loading
        $scope.$watch('project', function (id) {
            //TEST
            $scope.tree = populateTree({Name: "Matthis"});

            if (!id)
                return;

            disable();
            $project.load(id)
                .then(function (project) {
                    $scope.tree = populateTree(project);
                    enable();
                });
        });

        //editor
        $scope.editorControl = {};
        $scope.editorKeywords = " ";

        function enable() {
            console.log('enable');
        }

        function disable() {
            console.log('disable');
        }

        //compiler interface
        function notifyErrors(errors) {
            angular.forEach(errors, function (value, key) {
                $scope.console.add("ERROR: " + value.Message, "xs-console-error", function () {
                    selectFile(value.File, true, function () {
                        $timeout(function () {
                            $scope.editorControl.gotoLine(value.Line, value.Character);
                        }, 100);
                    });
                });
            });
        }

        $scope.compileProject = function () {
            if ($scope.compilerBusy)
                return;

            $scope.compilerBusy = true;
            startConsole("Compiling...");

            $scope.saveFiles();
            xsProject.compile()
                .then(function (result) {
                    var compilation = result.data;
                    if (compilation.Succeded)
                        $scope.console.add("Compiled Successfully");
                    else
                        notifyErrors(compilation.Errors);
                })
                .finally(function () {
                    $scope.compilerBusy = false;
                });
        };

        $scope.runProject = function () {
            if ($scope.compilerBusy)
                return;

            $scope.compilerBusy = true;
            startConsole("Executing...");

            $scope.saveFiles();
            xsProject.execute(consoleNotification)
                .then(function (result) {
                    var compilation = result.data;
                    if (compilation.Succeded) {
                        $scope.console.add("Ran Successfully");

                        if (compilation.ClientData) {
                            var debuggerDlg = compilation.ClientData.debuggerDlg;
                            var debuggerCtrl = compilation.ClientData.debuggerCtrl;
                            if (debuggerDlg && debuggerCtrl) {
                                var dlg = dialogs.create(debuggerDlg,
                                                         debuggerCtrl,
                                                         compilation.ClientData.debuggerData,
                                                         { size: "1200px" });
                            }
                        }
                    }
                    else
                        notifyErrors(compilation.Errors);
                })
                .finally(function () {
                    $scope.compilerBusy = false;
                });
        };

        //file management
        var _fileCache = {};

        function fileLoaded(file, contents, inNewTab) {
            if (inNewTab)
                addTab(file, contents);
            else
                defaultTab(file, contents);
        }

        function loadFile(file, inNewTab, callback) {
            var cached = _fileCache[file];
            if (cached) {
                fileLoaded(file, cached.contents, inNewTab);
                if (callback)
                    callback(file);
                return;
            }

            if ($scope.fileBusy)
                return;

            $scope.fileBusy = true;
            xsProject.loadFile(file)
                .then(function (result) {
                    _fileCache[file] =
                    {
                        changed: false,
                        contents: result.data
                    };

                    fileLoaded(file, result.data, inNewTab)
                    if (callback)
                        callback(file);
                })
                .finally(function () {
                    $scope.fileBusy = false;
                });
        }

        //tab management
        $scope.editors =
        [
            { id: 1, name: "_" },
        ];

        var _currentTab = null;
        $scope.tabSelected = function (selected) {
            if (_currentTab != selected) {
                _currentTab = selected;

                var cached = _fileCache[_currentTab.name];

                _ignoreChange = true;
                if (cached)
                    $scope.sourceCode = cached.contents;
                else
                    $scope.sourceCode = selected.sourceCode;
            }
        }

        function addTab(name, contents) {
            var id = $scope.editors.length + 1;
            $scope.editors.push({
                id: id,
                name: name,
                changed: false,
                sourceCode: contents,
                active: true
            });

            _ignoreChange = true;
        }

        function defaultTab(file, content) {
            _currentTab = $scope.editors[0];
            if (_currentTab.changed)
                saveFiles(true);

            _currentTab.changed = false;
            _currentTab.name = file;
            _currentTab.active = true;
            _currentTab.sourceCode = content;

            _ignoreChange = true;
            $scope.sourceCode = content;
        }

        //tree actions
        function populateTree(project) {
            return [{
                label: project.Name || "Project",
                icon: "glyphicon glyphicon-align-left",
                children: [{
                    label: "Map",
                    icon: "fa-map-o",
                    actions: [{
                        id: "edit-map",
                        icon: "glyphicon glyphicon-search",
                    }, ]
                }, ]
            },
            ];
        }

        function selectFile(file, inNewTab, callback) {
            var found = false;
            angular.forEach($scope.editors, function (value, key) {
                if (value.name == file) {
                    found = true;
                    value.active = true;

                    return;
                }
            });

            if (!found)
                loadFile(file, inNewTab, callback);
            else if (callback)
                callback(file);

        }

        $scope.projectAction = function (action, data) {
            switch (action) {
                case "select-file":
                    selectFile(data);
                    break;
                case "open-tab":
                    selectFile(data, true);
                    break;
                    //TODO: custom actions
            }
        }

        //console
        $scope.console = {};
        $scope.consoleOpen = false;

        function startConsole(text) {
            $scope.layoutControl.open('south');
            $scope.console.clear(text);
        }

        function consoleNotification(notification) {
            $scope.console.add(notification.Message);
        }
    }])

    .directive('xsuiIde', ['$parse', function ($parse) {
        return {
            restrict: 'AE',
            replace: true,
            controller: 'xsuiIdeController',
            scope: {
                options: '=',
                events: '&',
            },
            templateUrl: 'components/excess-ui-ide/excess-ui-ide.html',
        };
    }]);