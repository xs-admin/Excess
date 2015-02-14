angular.module('app')

.service('xsProject', ['$http', '$timeout', function ($http, $timeout) {

    this.loadProject = function (id) {
        return $http.get('/Project/LoadProject',
        {
            params: { projectId: id }
        });
    }

    this.loadFile = function (file) {
        return $http.get('/Project/LoadFile',
        {
            params: { file: file }
        });
    }

    this.saveFile = function (file, contents) {
        return $http.post('/Project/SaveFile', {
            file: file,
            contents: contents
        });
    }

    this.createClass = function (className) {
        return $http.get('/Project/CreateClass', {
            params:
            {
                className: className
            }
        });
    }

    this.createProject = function (projectName, projectType, projectData) {
        return $http.get('/Project/CreateProject', {
            params:
            {
                projectType: projectType,
                projectName: projectName,
                projectData: projectData
            }
        });
    }
    

    function getNotifications(notify) {

        function tick() {
            $http.get('/Project/Notifications')
                .then(function (result) {
                    var finished = false;

                    angular.forEach(result.data, function (value, key) {
                        notify(value);

                        if (value.Kind == 4) {
                            finished = true;
                        }
                    });

                    if (!finished)
                        $timeout(getNotifications, 100);
                })
        }

        $timeout(tick, 100);
    }

    this.userProjects = function () {
        return $http.get('/Project/UserProjects');
    }

    this.compile = function (notify) {
        getNotifications(notify);
        return $http.post('/Project/Compile');
    }

    this.execute = function (notify) {
        getNotifications(notify);
        return $http.post('/Project/Execute');
    }

    this.debugDSL = function (text) {
        return $http.get('/Project/debugDSL', {
            params: { text: text }
        });
    }

    this.dslTests = function () {
        return $http.get('/Project/GetDSLTests');
    }

    this.udateDslTest = function (test) {
        return $http.post('/Project/UpdateDSLTest', {
            id:       test.ID,
            contents: test.Contents
        });
    }

    this.addDslTest = function (name, contents) {
        return $http.post('/Project/AddDSLTest', {
            name:     name,
            contents: contents
        });
    }

    this.extensionItem = function (name, kind) {
        switch (kind)
        {
            case "lexical-transform":
                return '\nprivate static IEnumerable<SyntaxToken> ' + name + '(IEnumerable<SyntaxToken> input, ILexicalMatchResult<SyntaxToken, SyntaxNode> match, Scope scope)'
                     + '\n{'
                     + '\n}\n';

            case "lexical-extension":
                return '\nprivate static IEnumerable<SyntaxToken> ' + name + '(IEnumerable<SyntaxToken> input, Scope scope, LexicalExtension<SyntaxToken> extension)'
                     + '\n{'
                     + '\n}\n';

            case "extended-lexical-extension":
                return '\nprivate static SyntaxNode ' + name + '(SyntaxNode input, Scope scope, LexicalExtension<SyntaxToken> extension)'
                     + '\n{'
                     + '\n}\n';

            case "syntax-transform":
                return '\nprivate static SyntaxNode ' + name + '(SyntaxNode input, Scope scope)'
                     + '\n{'
                     + '\n}\n';

            case "syntax-extension":
                return '\nprivate static SyntaxNode ' + name + '(SyntaxNode input, Scope scope, SyntacticalExtension<SyntaxNode> extension)'
                     + '\n{'
                     + '\n}\n';

            case "semantical-transform":
                return '\nprivate static SyntaxNode ' + name + '(SyntaxNode input, SemanticModel model, Scope scope)'
                     + '\n{'
                     + '\n}\n';

            case "semantical-error-handler":
                return '\nprivate static void ' + name + '(SyntaxNode input, SemanticModel model, Scope scope)'
                     + '\n{'
                     + '\n}\n';

        }
    }
}]);