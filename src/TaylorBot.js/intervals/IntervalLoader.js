'use strict';

const fs = require('fs').promises;
const path = require('path');

const BaseInterval = require('./Interval.js');

const intervalsPath = __dirname;

class IntervalLoader {
    static async loadAll() {
        const dirEntries = await fs.readdir(intervalsPath, { withFileTypes: true });
        const subDirectories = dirEntries.filter(de => de.isDirectory());

        const filesInSubDirectories = [];

        for (const subDirectory of subDirectories.map(de => path.join(intervalsPath, de.name))) {
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
            .map(Interval => {
                if (!(Interval.prototype instanceof BaseInterval))
                    throw new Error(`Loading all intervals: interval ${Interval.name} doesn't extend base interval class.`);
                return new Interval();
            });
    }
}

module.exports = IntervalLoader;