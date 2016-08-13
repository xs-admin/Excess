'use strict';

angular.module('metaprogramming.version', [
  'metaprogramming.version.interpolate-filter',
  'metaprogramming.version.version-directive'
])

.value('version', '0.1');
