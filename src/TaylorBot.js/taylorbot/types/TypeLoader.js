'use strict';

const fs = require('fs').promises;
const path = require('path');

const ArgumentType = require('./ArgumentType.js');

const typesPath = __dirname;

class TypeLoader {
    static async loadAll() {
        const dirEntries = await fs.readdir(typesPath, { withFileTypes: true });
        const subDirectories = dirEntries.filter(de => de.isDirectory());

        const filesInSubDirectories = [];

        for (const subDirectory of subDirectories.map(de => path.join(typesPath, de.name))) {
            const subDirEntries = await fs.readdir(subDirectory, { withFileTypes: true });
            const subDirFiles = subDirEntries.filter(de => de.isFile());

            filesInSubDirectories.push(
                ...subDirFiles.map(f => path.join(subDirectory, f.name))
            );
        }

        return filesInSubDirectories
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(path.format)
            .map(require)
            .map(Type => {
                if (!(Type.prototype instanceof ArgumentType))
                    throw new Error(`Loading all types: type ${Type.name} doesn't extend base type class.`);
                return new Type();
            });
    }
}

module.exports = TypeLoader;