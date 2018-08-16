'use strict';

const path = require('path');

const PathMapper = require(path.join(__dirname, 'PathMapper'));

class GlobalPaths {
    constructor(rootPath) {
        const pathMapper = new PathMapper(rootPath);

        const mappedPaths = [
            {
                'directory': pathMapper.structures.path,
                'files': {
                    'EventHandler': 'EventHandler'
                }
            },
            {
                'directory': pathMapper.modules.path,
                'files': {
                    'DiscordFormatter': 'DiscordFormatter',
                    'DiscordEmbedFormatter': 'DiscordEmbedFormatter'
                }
            }
        ];

        for (const mappedPath of mappedPaths) {
            for (const propertyName in mappedPath.files) {
                this[propertyName] = path.join(mappedPath.directory, mappedPath.files[propertyName]);
            }
        }

        this.eventsFolderPath = pathMapper.events.path;
        this.intervalsFolderPath = pathMapper.intervals.path;
        this.watchersFolderPath = pathMapper.watchers.path;
        this.commandsFolderPath = pathMapper.commands.path;
        this.typesFolderPath = pathMapper.types.path;
        this.inhibitorsFolderPath = pathMapper.inhibitors.path;
        this.databaseScriptsPath = pathMapper.database.scripts.path;
    }
}

module.exports = GlobalPaths;