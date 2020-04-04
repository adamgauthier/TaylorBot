import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');

import BaseCommand = require('./Command.js');

const commandsPath = __dirname;

export class CommandLoader {
    static async loadAll(): Promise<BaseCommand[]> {
        const dirEntries = await fs.readdir(commandsPath, { withFileTypes: true });
        const subDirectories = dirEntries.filter(de => de.isDirectory());

        const filesInSubDirectories = [];

        for (const subDirectory of subDirectories.map(de => path.join(commandsPath, de.name))) {
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
            .map(Command => {
                if (!(Command.prototype instanceof BaseCommand))
                    throw new Error(`Loading all commands: command ${Command.name} doesn't extend base command class.`);
                return new Command();
            });
    }
}
