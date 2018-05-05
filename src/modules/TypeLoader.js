'use strict';

const fs = require('fs/promises');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const typesPath = GlobalPaths.typesFolderPath;

const requireType = typeName => require(path.join(typesPath, typeName));

class TypeLoader {
    static async loadAll() {
        const files = await fs.readdir(typesPath);

        const types = [];

        files.forEach(filename => {
            const filePath = path.parse(filename);
            if (filePath.ext === '.js') {
                const Type = requireType(filePath.base);
                types.push(Type);
            }
        });

        return types;
    }
}

module.exports = TypeLoader;