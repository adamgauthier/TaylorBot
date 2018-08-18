'use strict';

const path = require('path');

class GlobalPaths {
    constructor(rootPath) {
        const mappedPaths = [];

        for (const mappedPath of mappedPaths) {
            for (const propertyName in mappedPath.files) {
                this[propertyName] = path.join(mappedPath.directory, mappedPath.files[propertyName]);
            }
        }
    }
}

module.exports = GlobalPaths;