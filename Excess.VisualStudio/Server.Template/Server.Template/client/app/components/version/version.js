'use strict';

angular.module('$safeprojectname$.version', [
  '$safeprojectname$.version.interpolate-filter',
  '$safeprojectname$.version.version-directive'
])

.value('version', '0.1');
