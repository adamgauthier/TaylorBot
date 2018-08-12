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
                'directory': pathMapper.structures.path,
                'files': {
                    'EventHandler': 'EventHandler',
                    'Interval': 'Interval',
                    'Inhibitor': 'Inhibitor',
                    'ArgumentParsingError': 'ArgumentParsingError',
                    'ArgumentType': 'ArgumentType',
                    'MessageContext': 'MessageContext',
                    'CommandMessageContext': 'CommandMessageContext'
                }
            },
            {
                'directory': pathMapper.client.path,
                'files': {
                    'UserGroups': 'UserGroups.json',
                    'TaylorBotClient': 'TaylorBotClient',
                    'TaylorBotMasterClient': 'TaylorBotMasterClient'
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
                    'MessageWatcherRegistry': 'MessageWatcherRegistry',
                    'Registry': 'Registry'
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
                    'ArrayUtil': 'ArrayUtil',
                    'DiscordFormatter': 'DiscordFormatter',
                    'DiscordEmbedFormatter': 'DiscordEmbedFormatter',
                    'InstagramModule': 'InstagramModule',
                    'TumblrModule': 'TumblrModule',
                    'RedditModule': 'RedditModule',
                    'YoutubeModule': 'YoutubeModule',
                    'TypeLoader': 'TypeLoader',
                    'IntervalRunner': 'IntervalRunner',
                    'InhibitorLoader': 'InhibitorLoader',
                    'EmbedUtil': 'EmbedUtil',
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