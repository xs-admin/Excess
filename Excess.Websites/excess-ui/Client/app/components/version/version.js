'use strict';

angular.module('excess_ui.version', [
  'excess_ui.version.interpolate-filter',
  'excess_ui.version.version-directive'
])

.value('version', '0.1');
