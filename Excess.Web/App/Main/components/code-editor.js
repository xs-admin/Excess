angular.module('ui.xs.codemirror', []) 

.controller('cmController', ['$scope', '$attrs', function ($scope, $attrs) {
    
    var _mime = false;

    this.registerMime = function (extraKeywords)
    {
        if (_mime)
            return;

        _mime = true; //td: different keywords in the same page?
        extraKeywords = extraKeywords || "";

        function words(str) {
            var obj = {}, words = str.split(" ");
            for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
            return obj;
        }

        function tokenAtString(stream, state) {
            var next;
            while ((next = stream.next()) != null) {
                if (next == '"' && !stream.eat('"')) {
                    state.tokenize = null;
                    break;
                }
            }
            return "string";
        }

        CodeMirror.defineMIME("text/x-xs", {
            name: "clike",
            keywords: words("abstract as base break case catch checked class const continue" +
                    " default delegate do else enum event explicit extern finally fixed for" +
                    " foreach goto if implicit in interface internal is lock namespace new" +
                    " operator out override params private protected public readonly ref return sealed" +
                    " sizeof stackalloc static struct switch this throw try typeof unchecked" +
                    " unsafe using virtual void volatile while add alias ascending descending dynamic from get" +
                    " global group into join let orderby partial remove select set value var yield" +
                    " function method on typedef constructor property" + extraKeywords),
            blockKeywords: words("catch class do else finally for foreach if struct switch try while" + extraKeywords),
            builtin: words("Boolean Byte Char DateTime DateTimeOffset Decimal Double" +
                    " Guid Int16 Int32 Int64 Object SByte Single String TimeSpan UInt16 UInt32" +
                    " UInt64 bool byte char decimal double short int long object" +
                    " sbyte float string ushort uint ulong"),
            atoms: words("true false null"),
            hooks: {
                "@": function (stream, state) {
                    if (stream.eat('"')) {
                        state.tokenize = tokenAtString;
                        return tokenAtString(stream, state);
                    }
                    stream.eatWhile(/[\w\$_]/);
                    return "meta";
                }
            }
        });
    }
}])

.constant('cmConfig', {
    lineNumbers: true,
    matchBrackets: true,
    theme: "neat",
    mode: "text/x-xs",  
    tabSize: 4,
    indentUnit: 4,
})

.directive('xsCodeEditor', ['$parse', '$timeout', 'cmConfig',
    function ($parse, $timeout, config) {
    return {
        restrict: 'E',
        replace: false,
        controller: 'cmController',
        scope: {
            keywords: '@',
            resized:  '@',
            source:   '@',
            changed:  '&',
        },
        templateUrl: '/App/Main/components/code-editor.html',
        link: function (scope, element, attrs, ctrl) {

            function createCodeMirror(keywords)
            {
                ctrl.registerMime(keywords);

                var options = {};
                //td: parse options

                options = angular.extend({}, options, config);

                var textArea = element.find('textarea')[0];
                var codeEditor = CodeMirror.fromTextArea(textArea, options);

                var _fixedSize = false;
                if (angular.isDefined(attrs.size))
                {
                    var fixed  = scope.$eval(attrs.size);
                    _fixedSize = true;

                    codeEditor.setSize(fixed.width, fixed.height);
                    
                    $timeout(function () {
                        codeEditor.refresh();
                    });
                }

                scope.content = function () {
                    return codeEditor.getValue();
                }

                codeEditor.on("change", function () {
                    if (scope.changed)
                        $timeout(function () {
                            scope.changed();
                        });
                });

                scope.$watch("source", function (value) {
                    if (value)
                        codeEditor.setValue(value);
                });

                if (!_fixedSize)
                {
                    scope.$watch('resized', function (value) {
                        codeEditor.setSize(element.width(), element.height());
                    });
                }
            }

            if (angular.isDefined(attrs.keywords)) {
                element.hide();
                scope.$watch("keywords", function (value) {
                    if (value)
                    {
                        element.show();
                        createCodeMirror(value);
                    }
                });
            }
            else
                createCodeMirror("");

        }
    };
}])