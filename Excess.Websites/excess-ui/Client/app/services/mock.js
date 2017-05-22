//angular.module('mock.project', [])
/*angular.module('excess_ui', [])
    .service('$xsProject', ['$q', '$timeout', function ($q, $timeout) {
        
        this.tree = function () {
            return [];
        };

        this.file = function (id) {
            return "";
        };

        this.compile = function (listener) {
			return {
				then: function(callback) {
					var compilation = { 
						Succeded: true,
						Errors: [],
					};
					
					var result = { data: compilation };
					var finally_callback;
					
					$timeout(function() {
						callback(result);
						
						if (finally_callback)
							finally_callback();
					}, 2000);
					
					return {
						finally: function(callback) {
							finally_callback = callback;
						}
					}
				}
				
			}
        };
    }]);
*/