

angular
    .module('ui.ideeditor', ['ui.tree'])
    .directive('uiIdeEditor', [ function () {
        return {
            restrict: 'E',
            replace: true,
			scope: {
				theme: '@',
			},
			
            templateUrl: '/components/ui-excess-ide/ui-excess-ide.html',
			controller: 'ui.ideeditor.controller',
        }
    }]);
	
(function () {
    var controllerId = 'ui.ideeditor.controller';
    angular.module('ui.ideeditor').controller(controllerId, [
/*        '$scope', '$rootScope', '$window', '$stateParams', '$timeout', 'dialogs', 'hotkeys', 'xsProject',
        function ($scope, $rootScope, $window, $stateParams, $timeout, dialogs, hotkeys, xsProject) {*/
        '$scope', '$rootScope', '$uibModal', '$window', '$timeout', 'excess_ui.Project',//'$xsProject',
        function ($scope, $rootScope, $uibModal, $window, $timeout, xsProject) {
            var vm = this;
			
	        var modeList = ace.require("ace/ext/modelist");
			
			$scope.getModeForPath = function(path) {
				return modeList ? modeList.getModeForPath(path).name : 'text';
			}
			
			$scope.openModalNewFile = function() {
				var modalInstance = $uibModal.open({
					templateUrl: 'NewFileModal.html',
					controller:  [ '$scope', function($scope) {
						$scope.newFileName = "File1.txt";
					}],
					controllerAs: '$ctrl',
					//size: size,
					//appendTo: parentElem,
					/*resolve: {
						items: function () {
						  return $ctrl.items;
						}
					}*/
				});
				
				/*modalInstance.result.then(function (newFileName) {
					console.log('bien: ' + newFileName);
				}, function () {
					console.log('cancelado');
				});*/
				
				return modalInstance.result;
			}
			
            //xsProject.initNotifications(consoleNotification);
			
            $scope.projectTree = []; 
			
			projectFind = function(fileID, callback, parent) {
				if (!parent)
					parent = $scope.projectTree;
				
				return parent.some(function(node, index) {
					if (node.data == fileID) {
						callback(node, parent, index)
						return true;
					}
					
					if (node.children && node.children.length > 0 && projectFind(fileID, callback, node.children))
						return true;
				});
			}
			
			// tabs management
			$scope.tabs = [];
			$scope.currentTab = undefined;

			function findTabIndex(id) {
				var tabIndex = -1;
				$scope.tabs.some(function(tab, index) {
					if (tab.id == id) {
						tabIndex = index;
						return true;
					}
				});
				
				return tabIndex;
			}
			
			function findTab(id) {
				var index = findTabIndex(id);
				if (index >= 0)
					return $scope.tabs[index];
			}
			
			function updateSourceModified() {
				$scope.sourceModified = $scope.tabs.some(function(tab) {
					return tab.modified;
				});
			}

			function addTab(fileID, name, contents) {
				$scope.tabs.push({
					id: fileID,
					name: name,
					sourceCode: contents,
				});

				$timeout(function() { $scope.activeTabID = fileID });
			}
			
			$scope.closeTab = function(id) {
				var tabIndex = findTabIndex(id);
				
				if (tabIndex >= 0) {
					if ($scope.tabs[tabIndex].modified) {
						;
					}
					$scope.tabs.splice(tabIndex, 1);
					updateSourceModified();
				}
			}
			
			function fileLoaded(fileID, name, contents, inNewTab) {
				if (inNewTab)
					addTab(fileID, name, contents);
				/*else
					defaultTab(file, contents);*/
			}
			
			function loadFile(fileID, inNewTab, callback) {
				/*var cached = _fileCache[file];
				if (cached) {
					fileLoaded(file, cached.contents, inNewTab);
					if (callback)
						callback(file);
					return;
				}*/

				if ($scope.fileBusy)
					return;

				//console.log('load-file: ' + fileID);
				
				$scope.fileBusy = true;
				xsProject.readFile(fileID)
					.then(function (result) {
						/*_fileCache[fileID] =
						{
							modified: false,
							contents: result.data
						};*/

						fileLoaded(fileID, result.name, result.content, inNewTab)
						/*if (callback)
							callback(fileID);*/
					})
					.finally(function () {
						$scope.fileBusy = false;
					});
			}
			
			deleteFile = function(fileID) {
				if ($scope.fileBusy)
					return;

				//console.log('delete-file: ' + fileID);
				
				$scope.fileBusy = true;
				xsProject.deleteFile(fileID)
					.then(function (result) {
						if (result) {
							var fileName;
							
							// delete node from projectTree
							projectFind(fileID, function(node, parent, index) {
								fileName = node.label;
								parent.splice(index, 1);
							});

							$scope.console.add(fileName + ': file deleted');
							
							// delete the tab
							var index = findTabIndex(fileID);
							if (index >= 0) {
								$scope.tabs.splice(index, 1);
								updateSourceModified();
							}
						}
					})
					.finally(function () {
						$scope.fileBusy = false;
					});
			}
			
			newFile = function(parentFileID, fileName) {
				if ($scope.fileBusy)
					return;

				$scope.fileBusy = true;
				xsProject.newFile(parentFileID, fileName)
					.then(function (result) {
						var fileDescriptor = result;
						if (fileDescriptor) {
							$scope.console.add(fileName + ': new file created');
							
							projectFind(parentFileID, function(node) {
								node.children.push(fileDescriptor);
							});
							
							// open in new tab
							addTab(fileDescriptor.data, fileName, "");
						}
						else
							$scope.console.add(fileName + ': error creating file');
					})
					.finally(function () {
						$scope.fileBusy = false;
					});
			}
			
			$scope.projectAction = function (action, fileID) { // data is the fileID
				switch (action) {
					case "read-file":
						var index = findTabIndex(fileID);
						if (index < 0)
							loadFile(fileID, true);
						else
							$scope.activeTabID = fileID;
						break;
					case "delete":
						deleteFile(fileID);
						break;
					case "new-file":
						$scope.openModalNewFile().then(function (newFileName) {
							newFile(fileID, newFileName);
						});
						break;
					/*case "select-file":
						selectFile(fileID);
						break;
					case "open-tab":
						selectFile(fileID, true);
						break;*/
					//TODO: custom actions
				}
			}

            //loading
            $scope.busy = true;
            $scope.fileBusy = false;
            $scope.compilerBusy = false;
            //$rootScope.$broadcast('loading-requests', 1);
 
            //load project
            xsProject.load('projects')//$stateParams.projectId)
                .then(function(result) {
                    $scope.busy = false;
                    //debugger;
                    $scope.projectTree = [result.root];//tree;

                    //loadFile(result.data.defaultFile);
                })
                .finally(function () {
                //    $rootScope.$broadcast('request-loaded');
                });
				

			$scope.onEditorChange = function(a)	{
				var editor = a[1];
				var tab = findTab(editor.container.attributes['fileID'].value);
				if (tab)
					tab.modified = !editor.session.getUndoManager().isClean();

				updateSourceModified();
			}
			
			getEditor = function(fileID) {
				var elem = document.getElementById("editor" + fileID);
				return elem && ace.edit(elem);
			}
			
			$scope.$watch('activeTabID', function(newVal) {
				$scope.currentTab = undefined;
				if (newVal != undefined) {
					$scope.currentTab = findTab(newVal);
					if ($scope.currentTab) {
						// focus to the editor
						getEditor(newVal).focus();
					}
				}
			});
			
			//$timeout(function() { $scope.activeTabID = 0; });
            
            //editor
            $scope.editorControl  = {};
            $scope.editorKeywords = " ";

            //layout
			$scope.layoutControl = {
				tree: false,
				south: false,
				
				toggle: function(which) {
					this[which] = !this[which];
				},

				close: function(which) {
					this[which] = true;
				},

				open: function(which) {
					this[which] = false;
				},
			};
			

            //console
            $scope.console = {
				lines: [],
				
				add: function(line) {
					this.lines.push(line);
					$timeout(function() { document.getElementById('console').scrollTop = 100000000; });

				},
				
				clear: function(text) {
					this.lines = [];
					this.add(text);
				},
				
			};

            function startConsole(text)
            {
                $scope.layoutControl.open('south');
                $scope.console.clear(text);
            }

            function consoleNotification(notification) {
                $scope.console.add(notification.Message);
            }

            //compiler interface
            /*function notifyErrors(errors) {
                angular.forEach(errors, function (value, key) {
                    $scope.console.add("ERROR: " + value.Message, "xs-console-error", function () {
                        selectFile(value.File, true, function () {
                            $timeout(function () {
                                $scope.editorControl.gotoLine(value.Line, value.Character);
                            }, 100);
                        });
                    });
                });
            }*/

			startConsole();
			
            $scope.saveCurrent = function() {
				if ($scope.currentTab && $scope.currentTab.modified) {
					var editor = getEditor($scope.currentTab.id);
					
					xsProject.saveFile($scope.currentTab.id, editor.getValue());
					$scope.console.add($scope.currentTab.name + " saved.");
					
					var undo = editor.session.getUndoManager();
					undo.markClean();
					$scope.currentTab.modified = !undo.isClean();
					
					updateSourceModified();
				}
			}
			
            $scope.saveFileAll = function() {
				$scope.tabs.forEach(function(tab) {
					if (tab.modified) {
						var editor = getEditor(tab.id);
						
						xsProject.saveFile(tab.id, editor.getValue());
						$scope.console.add(tab.name + " saved.");
						
						var undo = editor.session.getUndoManager();
						undo.markClean();
						tab.modified = !undo.isClean();
					}
				});
				
                $scope.sourceModified = false;
            }
			
            $scope.compileProject = function () {
                /*if ($scope.compilerBusy)
                    return;

                $scope.compilerBusy = true;
                startConsole("Compiling...");
                
                //$scope.saveFileAll();
                xsProject.compile(getEditor(0).getValue())
                    .then(function (result) {

                        var compilation = result.data;
                        if (compilation.Succeded)
                            $scope.console.add("Compiled Successfully");
                        else
                            notifyErrors(compilation.Errors);
                    })
                    .finally(function () {
                        $scope.compilerBusy = false;
                    });*/
            };

            $scope.runProject = function () {
                if ($scope.compilerBusy)
                    return;

                $scope.compilerBusy = true;
                startConsole("Executing...");

				$timeout(function() { $scope.console.add('Ran Successfully'); $scope.compilerBusy = false; }, 2000);
                /*$scope.saveFileAll();
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
                    });*/
            };

			return;
			
			/*
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
            
            //show help on first visit
            var hasVisited = $window.localStorage['xs-seen-project-help'];
            if (!hasVisited) {
                $window.localStorage['xs-seen-project-help'] = true;
                $rootScope.projectHelp();
            }*/
        }
    ])
})();