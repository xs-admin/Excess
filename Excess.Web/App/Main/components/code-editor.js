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

.directive('xsCodeEditor', ['$parse', 'cmConfig', function ($parse, config) {
    return {
        restrict: 'E',
        replace: false,
        controller: 'cmController',
        scope: {
            resized: '@',
            source:  '@',
            changed: '&',
        },
        templateUrl: '/App/Main/components/code-editor.html',
        link: function (scope, element, attrs, ctrl) {
            var extraKeywords = angular.isDefined(attrs.ngExtraKeywords) ? scope.$eval(attrs.ngExtraKeywords) : "";
            ctrl.registerMime(extraKeywords);

            var options = {};
            //td: parse options

            options = angular.extend({}, options, config);

            var textArea   = element.find('textarea')[0];
            var codeEditor = CodeMirror.fromTextArea(textArea, options);

            scope.content = function () {
                return codeEditor.getValue();
            }

            codeEditor.on("change", function ()
            {
                if (scope.changed)
                    scope.$apply(function () {
                        scope.changed();
                    });
            });

            scope.$watch("source", function (value) {
                if (value)
                    codeEditor.setValue(value);
            });

            scope.$watch('resized', function (value) {
                codeEditor.setSize(element.width(), element.height());
            });
        }
    };
}])