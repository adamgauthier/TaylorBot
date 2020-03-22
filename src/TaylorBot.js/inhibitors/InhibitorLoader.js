'use strict';

const fs = require('fs').promises;
const path = require('path');

const Inhibitor = require('./Inhibitor.js');

const inhibitorsPath = __dirname;

class InhibitorLoader {
    static async loadAll() {
        const dirEntries = await fs.readdir(inhibitorsPath, { withFileTypes: true });
        const subDirectories = dirEntries.filter(de => de.isDirectory());

        const filesInSubDirectories = [];

        for (const subDirectory of subDirectories.map(de => path.join(inhibitorsPath, de.name))) {
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
                if (!(Type.prototype instanceof Inhibitor))
                    throw new Error(`Loading all inhibitors: inhibitor ${Type.name} doesn't extend base inhibitor class.`);
                return new Type();
            });
    }
}

module.exports = InhibitorLoader;