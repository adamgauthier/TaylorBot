'use strict';

const fs = require('fs').promises;
const path = require('path');

const typesPath = path.join(__dirname, '..', 'types');

const requireType = typeName => require(path.join(typesPath, typeName));

class TypeLoader {
    static async loadAll() {
        const files = await fs.readdir(typesPath);

        return files
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(file => {
                const Type = requireType(file.base);
                return new Type();
            });
    }
}

module.exports = TypeLoader;