'use strict';

const { GlobalPaths } = require('globalobjects');

class ArgumentInfo {
    constructor(typeId, name) {
        this.typeId = typeId;
        this.name = name;
    }
}

module.exports = ArgumentInfo;