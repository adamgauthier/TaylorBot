'use strict';

const fs = require('fs');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const commandsPath = GlobalPaths.commandsFolderPath;

const requireCommand = commandName => require(path.join(commandsPath, commandName));

class CommandLoader {
    static loadAll() {
        return new Promise((resolve, reject) => {
            fs.readdir(commandsPath, (err, files) => {
                if (err) reject(err);
                else {
                    const commands = {};
                    files.forEach(filename => {
                        const filePath = path.parse(filename);
                        if (filePath.ext === '.js') {
                            const command = requireCommand(filePath.base);
                            const commandName = filePath.name.toLowerCase();
                            command.name = commandName;
                            commands[commandName] = command;
                        }
                    });
                    resolve(commands);
                }
            });
        });
    }
}

module.exports = CommandLoader;