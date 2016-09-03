'use strict';

describe('excess_ui.version module', function () {
  beforeEach(module('excess_ui.version'));

  describe('version service', function() {
    it('should return current version', inject(function(version) {
      expect(version).toEqual('0.1');
    }));
  });
});
