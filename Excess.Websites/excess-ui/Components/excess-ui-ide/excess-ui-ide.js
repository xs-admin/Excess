angular.module('excess.ui')
    .controller('xsuiIdeController', ['$scope', '$project', function ($scope, $project) {
        var vm = this;

        //status
        $scope.busy = true;
        $scope.fileBusy = false;
        $scope.compilerBusy = false;

        //loading
        $scope.watch('project', function (id) {
            disable();
            $project.load(id)
                .then(function (project) {
                    populateTree(project);
                    enable();
                });
        });

        //editor
        $scope.editorControl = {};
        $scope.editorKeywords = " ";

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
    }]);


(function () {
    var controllerId = 'app.views.project';
    angular.module('app').controller(controllerId, [
        '$scope', '$rootScope', '$window', '$stateParams', '$timeout', 'dialogs', 'hotkeys', 'xsProject',
        function ($scope, $rootScope, $window, $stateParams, $timeout, dialogs, hotkeys, xsProject) {
            var vm = this;
            
            xsProject.initNotifications(consoleNotification);

            //project tree
            $scope.projectTree = null;

            //loading
            $scope.busy = true;
            $scope.fileBusy = false;
            $scope.compilerBusy = false;
            $rootScope.$broadcast('loading-requests', 1);

            //load project
            xsProject.loadProject($stateParams.projectId)
                .then(function(result) {
                    $scope.busy = false;
                    $scope.projectTree = result.data.tree;

                    loadFile(result.data.defaultFile);
                })
                .finally(function () {
                    $rootScope.$broadcast('request-loaded');
                });

            
            //editor
            $scope.editorControl  = {};
            $scope.editorKeywords = " ";

            //layout
            $scope.layoutControl = {};

            //console
            $scope.console = {};
            $scope.consoleOpen = false;

            function startConsole(text)
            {
                $scope.layoutControl.open('south');
                $scope.console.clear(text);
            }

            function consoleNotification(notification) {
                $scope.console.add(notification.Message);
            }

            //compiler interface
            function notifyErrors(errors)
            {
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
                    .then(function (result)
                    {
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
                        if (compilation.Succeded)
                        {
                            $scope.console.add("Ran Successfully");

                            if (compilation.ClientData)
                            {
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

            //keyboard shortcuts
            hotkeys.add({
                combo: 'ctrl+shift+b',
                description: 'Compile',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function (event) {
                    $scope.compileProject();
                    event.preventDefault();
                }
            });

            hotkeys.add({
                combo: 'ctrl+f5',
                description: 'Run',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function (event) {
                    $scope.runProject();
                    event.preventDefault();
                }
            });

            hotkeys.add({
                combo: 'ctrl+s',
                description: 'Save',
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                callback: function (event) {
                    $scope.saveFiles();
                    event.preventDefault();
                }
            });

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

            $scope.appendSources = function (files) {
                angular.forEach(files, function (contents, file) {
                    var cache = _fileCache[file];
                    if (cache) {
                        cache.contents += contents;
                        cache.changed = true;

                        if (_currentTab.name == file)
                            $scope.sourceCode = cache.contents;
                    }
                    else loadFile(file, false, function () {
                        cache = _fileCache[file];
                        cache.contents += contents;
                        cache.changed = true;

                        if (_currentTab.name == file)
                            $scope.sourceCode = cache.contents;
                    })
                });
            };

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
            
            //show help on first visit
            var hasVisited = $window.localStorage['xs-seen-project-help'];
            if (!hasVisited) {
                $window.localStorage['xs-seen-project-help'] = true;
                $rootScope.projectHelp();
            }
        }
    ])
})();