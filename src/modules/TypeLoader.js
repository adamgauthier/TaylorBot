'use strict';

const fs = require('fs');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const typesPath = GlobalPaths.typesFolderPath;

const requireType = typeName => require(path.join(typesPath, typeName));

class TypeLoader {
    static loadAll() {
        return new Promise((resolve, reject) => {
            fs.readdir(typesPath, (err, files) => {
                if (err) reject(err);
                else {
                    const types = [];
                    files.forEach(filename => {
                        const filePath = path.parse(filename);
                        if (filePath.ext === '.js') {
                            const Type = requireType(filePath.base);
                            types.push(Type);
                        }
                    });
                    resolve(types);
                }
            });
        });
    }
}

module.exports = TypeLoader;