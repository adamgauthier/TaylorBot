import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');
import { NoisyInhibitor } from './NoisyInhibitor';
import { SilentInhibitor } from './SilentInhibitor';

const inhibitorsPath = __dirname;

export class InhibitorLoader {
    static async loadAll(): Promise<(SilentInhibitor | NoisyInhibitor)[]> {
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
            .map((Type: new () => (SilentInhibitor | NoisyInhibitor)) => {
                return new Type();
            });
    }
}
