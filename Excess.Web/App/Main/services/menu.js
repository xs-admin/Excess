angular.module('app')

.service('xsMenu', ['$rootScope', '$state', '$modal', 'dialogs', 'xsProject', 'xsAuthentication',
    function ($rootScope, $state, $modal, dialogs, project, auth) {

        $rootScope.selectProject = function ()
        {
            if (!auth.isAuthenticated())
            {
                dialogs.error("Error",
                              "Must be logged in order to access your projects", 
                              { size: "sm" });
                return;
            }

            dialogs.create('/App/Main/dialogs/projectList.html', 'projectListCtrl', null, { size: "md" });
        }

        $rootScope.newProject = function () {
            if (!auth.isAuthenticated()) {
                dialogs.error("Error",
                              "Must be logged in order to create projects",
                              { size: "sm" });
                return;
            }

            dialogs.create('/App/Main/dialogs/newProject.html', 'newProjectCtrl', null, { size: "md" })
                .result.then(function (projectId) {
                    dialogs.confirm("Project Created",
                                  "Would you like to edit this project?", { size: "sm" })
                        .result.then(function () {
                            $state.go('project', { projectId: projectId });
                        })
                });
        }

        this.headerMenu = function (state) {
            var result = [];
            switch (state)
            {
                case 'home':
                {
                    result = [
                        { icon: 'fa-sitemap',     action: $rootScope.selectProject },
                        { icon: 'fa-plus-square', action: $rootScope.newProject },
                        //{ icon: 'fa-info',        url: '#/about' }
                    ];
                    break;
                }

                case 'project': 
                {
                    result = [
                        { icon: 'fa-home',        url: '#/' },
                        { icon: 'fa-sitemap',     action: $rootScope.selectProject },
                        { icon: 'fa-plus-square', action: $rootScope.newProject },
                        //{ icon: 'fa-info',        url: '#/about' }
                    ];
                    break;
                }

                default: alert("Invalid state for menu");
            }

            return result;
        }
    }
]);