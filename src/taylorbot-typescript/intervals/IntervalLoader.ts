import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');

import { Interval } from './Interval';

const intervalsPath = __dirname;

export class IntervalLoader {
    static async loadAll(): Promise<Interval[]> {
        const dirEntries = await fs.readdir(intervalsPath, { withFileTypes: true });
        const subDirectories = dirEntries.filter(de => de.isDirectory());

        const filesInSubDirectories: string[] = [];

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
            .map(file => require(file))
            .map((loaded: new () => Interval) => new loaded());
    }
}
