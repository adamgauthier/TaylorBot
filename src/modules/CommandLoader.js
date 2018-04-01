'use strict';

const fs = require('fs');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const commandsPath = GlobalPaths.commandsFolderPath;

const requireCommand = commandName => require(path.join(commandsPath, commandName));

class CommandLoader {
    static loadAll(client) {
        return new Promise((resolve, reject) => {
            fs.readdir(commandsPath, (err, files) => {
                if (err) reject(err);
                else {
                    const obj = require('require-all')(commandsPath);
                    const commands = {};
                    for (const group of Object.values(obj)) {
                        for (const Command of Object.values(group)) {
                            const command = new Command(client);
                            commands[command.name] = command;
                        }
                    }
                    resolve(commands);
                }
            });
        });
    }
}

module.exports = CommandLoader;