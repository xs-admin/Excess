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

            dialogs.create('/App/Main/dialogs/createProject.html', 'createProjectCtrl', null, { size: "md" })
                .result.then(function (projectId) {
                    dialogs.confirm("Project Created", "Would you like to edit this project?", { size: "sm" })
                        .result.then(function () {
                            $state.go('project', { projectId: projectId });
                        })
                });
        }

        $rootScope.homeHelp = function () {
            var homeHelpTopics = [
                { caption: 'Usage', visible: true,  image: '/Content/images/home-usage.png',   helpText: '' },
                { caption: 'Test driving the platform', visible: false, image: '/Content/images/home-options.png', helpText: 'It compiler whatever mofifications you make, too.' },
                { caption: 'Can I write compilers here?', visible: false, image: '', helpText: 'Yes.' },
                {
                    caption: 'What kind of projects can I create?', visible: false, image: '',
                    helpText: '<i class="fa fa-signal"></i> DSL Projects create compilers for c# extensions <br/>'
                            + '<i class="fa fa-terminal"></i> Console projects are simple textual apps for testing purposes'
                },              
                { caption: 'How cool is this?', visible: false, image: '', helpText: 'Very?' },
                { caption: 'How much documentation is there?', visible: false, image: '', helpText: 'Not much, working on it' },
            ];

            dialogs.create('/App/Main/dialogs/helpDialog.html', 'helpDialogCtrl', homeHelpTopics, { size: "md" });
        }

        $rootScope.projectHelp = function () {
            var projectHelpTopics = [
                { caption: 'Usage', visible: true, image: '/Content/images/project-usage.png', helpText: '' },
                {
                    caption: 'How do I debug my compilers', visible: false, image: '/Content/images/dsl-debugger.png',
                    helpText: 'Use this dialog after running (CTRL+F5) your DSL application. <br/>'
                            + 'You will also receive errors and debug info via <strong>console.write</strong>'
                },
            ];

            dialogs.create('/App/Main/dialogs/helpDialog.html', 'helpDialogCtrl', projectHelpTopics, { size: "md" });
        }

        this.headerMenu = function (state) {
            var result = [];
            switch (state)
            {
                case 'home':
                {
                    result = [
                        { icon: 'fa-sitemap',         action: $rootScope.selectProject },
                        { icon: 'fa-plus-square',     action: $rootScope.newProject    },
                        { icon: 'fa-info-circle',     url: '#/about' },
                        { icon: 'fa-question-circle', action: $rootScope.homeHelp },
                    ];
                    break;
                }

                case 'project': 
                {
                    result = [
                        { icon: 'fa-home',            url: '#/' },
                        { icon: 'fa-sitemap',         action: $rootScope.selectProject },
                        { icon: 'fa-plus-square',     action: $rootScope.newProject },
                        { icon: 'fa-question-circle', action: $rootScope.projectHelp },
                    ];
                    break;
                }

                case 'about':
                {
                    result = [
                        { icon: 'fa-home', url: '#/' },
                    ];
                    break;
                }

                default: alert("Invalid state for menu");
            }

            return result;
        }
    }
]);