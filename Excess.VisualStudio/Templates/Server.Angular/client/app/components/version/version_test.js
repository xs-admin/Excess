'use strict';

describe('$safeprojectname$.version module', function () {
  beforeEach(module('$safeprojectname$.version'));

  describe('version service', function() {
    it('should return current version', inject(function(version) {
      expect(version).toEqual('0.1');
    }));
  });
});
