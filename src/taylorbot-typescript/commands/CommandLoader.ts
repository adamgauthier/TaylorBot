import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');

import { Command } from './Command';

const commandsPath = __dirname;

export class CommandLoader {
    static async loadAll(): Promise<Command[]> {
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
            .map(CommandType => {
                if (!(CommandType.prototype instanceof Command))
                    throw new Error(`Loading all commands: command ${CommandType.name} doesn't extend base command class.`);
                return new CommandType();
            });
    }
}
