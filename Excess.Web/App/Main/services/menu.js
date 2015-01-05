angular.module('app')

.service('xsMenu', ['$rootScope', '$state', 'dialogs', 'xsProject', 'xsAuthentication',
    function ($rootScope, $state, dialogs, project, auth) {

        $rootScope.selectProject = function ()
        {
            if (!auth.isAuthenticated())
            {
                dialogs.error("Error",
                              "Must be logged in order to access your projects", 
                              { size: "sm" });
                return;
            }

            project.userProjects()
                .then(function (result) {
                    dialogs.create('/App/Main/dialogs/selectProject.html', 'selectProjectCtrl',
                                   result.data, { size: "md" })
                        .then(function (selected) {
                            alert('GO PROJ' + selected);
                        });
                });
        }

        $rootScope.newProject = function () {
            if (!auth.isAuthenticated()) {
                dialogs.error("Error",
                              "Must be logged in order to create projects",
                              { size: "sm" });
                return;
            }

            dialogs.create('/App/Main/dialogs/newProject.html', 'newProjectCtrl', null, { size: "md" })
                .then(function (selected) {
                    alert('CREATE PROJ' + selected);
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
                        { icon: 'fa-info',        url: '#/about' }
                    ];
                    break;
                }

                case 'project': 
                {
                    result = [
                        { icon: 'fa-home',        url: '#/' },
                        { icon: 'fa-sitemap',     action: $rootScope.selectProject },
                        { icon: 'fa-plus-square', action: $rootScope.newProject },
                        { icon: 'fa-info',        url: '#/about' }
                    ];
                    break;
                }

                default: alert("Invalid state for menu");
            }

            return result;
        }
    }
]);