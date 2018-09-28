'use strict';

const fs = require('fs').promises;
const path = require('path');

const BaseCommand = require('../structures/Command.js');

const commandsPath = __dirname;

class CommandLoader {
    static async loadAll() {
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

module.exports = CommandLoader;