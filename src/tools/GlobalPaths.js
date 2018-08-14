'use strict';

const path = require('path');

const PathMapper = require(path.join(__dirname, 'PathMapper'));

class GlobalPaths {
    constructor(rootPath) {
        const pathMapper = new PathMapper(rootPath);

        const mappedPaths = [
            {
                'directory': pathMapper.config.path,
                'files': {
                    'GoogleConfig': 'google.json',
                    'CleverBotConfig': 'cleverbot.json',
                    'WolframConfig': 'wolfram.json',
                    'ImgurConfig': 'imgur.json',
                    'MyApiFilmsConfig': 'myapifilms.json'
                }
            },
            {
                'directory': pathMapper.structures.path,
                'files': {
                    'EventHandler': 'EventHandler',
                    'Interval': 'Interval',
                    'ArgumentType': 'ArgumentType'
                }
            },
            {
                'directory': pathMapper.client.registry.path,
                'files': {
                    'GuildRegistry': 'GuildRegistry',
                    'UserRegistry': 'UserRegistry',
                    'InhibitorRegistry': 'InhibitorRegistry',
                    'CommandRegistry': 'CommandRegistry',
                    'CachedCommand': 'CachedCommand',
                    'TypeRegistry': 'TypeRegistry',
                    'GroupRegistry': 'GroupRegistry',
                    'GuildRoleGroupRegistry': 'GuildRoleGroupRegistry',
                    'MessageWatcherRegistry': 'MessageWatcherRegistry'
                }
            },
            {
                'directory': pathMapper.modules.path,
                'files': {
                    'StringUtil': 'StringUtil',
                    'MathUtil': 'MathUtil',
                    'DiscordFormatter': 'DiscordFormatter',
                    'DiscordEmbedFormatter': 'DiscordEmbedFormatter',
                    'InstagramModule': 'InstagramModule',
                    'TumblrModule': 'TumblrModule',
                    'RedditModule': 'RedditModule',
                    'YoutubeModule': 'YoutubeModule',
                    'TypeLoader': 'TypeLoader',
                    'CommandLoader': 'CommandLoader'
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