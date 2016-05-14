'use strict';

describe('metaprogramming.version module', function () {
  beforeEach(module('metaprogramming.version'));

  describe('version service', function() {
    it('should return current version', inject(function(version) {
      expect(version).toEqual('0.1');
    }));
  });
});
