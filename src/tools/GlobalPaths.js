'use strict';

const path = require('path');

const PathMapper = require(path.join(__dirname, 'PathMapper'));

class GlobalPaths {
    constructor(rootPath) {
        const pathMapper = new PathMapper(rootPath);

        const mappedPaths = [
            {
                'directory': pathMapper.modules.path,
                'files': {
                    'DiscordFormatter': 'DiscordFormatter'
                }
            }
        ];

        for (const mappedPath of mappedPaths) {
            for (const propertyName in mappedPath.files) {
                this[propertyName] = path.join(mappedPath.directory, mappedPath.files[propertyName]);
            }
        }

        this.intervalsFolderPath = pathMapper.intervals.path;
        this.watchersFolderPath = pathMapper.watchers.path;
        this.commandsFolderPath = pathMapper.commands.path;
        this.typesFolderPath = pathMapper.types.path;
        this.inhibitorsFolderPath = pathMapper.inhibitors.path;
    }
}

module.exports = GlobalPaths;