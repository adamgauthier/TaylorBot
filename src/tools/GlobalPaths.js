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
                    'TaylorBotConfig': 'config.json',
                    'DiscordConfig': 'discord.json',
                    'TumblrConfig': 'tumblr.json',
                    'GoogleConfig': 'google.json',
                    'CleverBotConfig': 'cleverbot.json',
                    'WolframConfig': 'wolfram.json',
                    'ImgurConfig': 'imgur.json',
                    'MyApiFilmsConfig': 'myapifilms.json',
                    'PostgreSQLConfig': 'postgresql.json'
                }
            },
            {
                'directory': pathMapper.database.path,
                'files': {
                    'DatabaseDriver': 'DatabaseDriver'
                }
            },
            {
                'directory': pathMapper.database.repositories.path,
                'files': {
                    'GuildRepository': 'GuildRepository'
                }
            },
            {
                'directory': pathMapper.structures.path,
                'files': {
                    'EventHandler': 'EventHandler',
                    'Interval': 'Interval',
                    'MessageWatcher': 'MessageWatcher',
                    'Command': 'Command',
                    'Inhibitor': 'Inhibitor'
                }
            },
            {
                'directory': pathMapper.client.path,
                'files': {
                    'UserGroups': 'UserGroups.json',
                    'GuildRegistry': 'GuildRegistry',
                    'UserRegistry': 'UserRegistry',
                    'CommandRegistry': 'CommandRegistry',
                    'TypeRegistry': 'TypeRegistry',
                    'GroupRegistry': 'GroupRegistry',
                    'GuildRoleGroupRegistry': 'GuildRoleGroupRegistry',
                    'Registry': 'Registry',
                    'taylorBotClient': 'TaylorBotClient'
                }
            },
            {
                'directory': pathMapper.tools.path,
                'files': {
                    'Logger': 'Logger'
                }
            },
            {
                'directory': pathMapper.modules.path,
                'files': {
                    'StringUtil': 'StringUtil',
                    'TimeUtil': 'TimeUtil',
                    'MathUtil': 'MathUtil',
                    'DiscordFormatter': 'DiscordFormatter',
                    'DiscordEmbedFormatter': 'DiscordEmbedFormatter',
                    'EventLoader': 'EventLoader',
                    'InstagramModule': 'InstagramModule',
                    'TumblrModule': 'TumblrModule',
                    'RedditModule': 'RedditModule',
                    'YoutubeModule': 'YoutubeModule',
                    'TypeLoader': 'TypeLoader',
                    'IntervalRunner': 'IntervalRunner',
                    'InhibitorLoader': 'InhibitorLoader',
                    'MessageWatcherRegistry': 'MessageWatcherRegistry',
                    'EmbedUtil': 'EmbedUtil'
                }
            },
            {
                'directory': pathMapper.constants.path,
                'files': {
                    'ArgumentInfos': 'ArgumentInfos'
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