'use strict';

const path = require('path');

const PathMapper = require(path.join(__dirname, 'PathMapper'));

class GlobalPaths {
    constructor(rootPath) {
        const pathMapper = new PathMapper(rootPath);

        // Config
        this.TaylorBotConfig = path.join(pathMapper.config.path, 'config.json');
        this.DiscordConfig = path.join(pathMapper.config.path, 'discord.json');
        this.TumblrConfig = path.join(pathMapper.config.path, 'tumblr.json');
        this.GoogleConfig = path.join(pathMapper.config.path, 'google.json');
        this.CleverBotConfig = path.join(pathMapper.config.path, 'cleverbot.json');
        this.WolframConfig = path.join(pathMapper.config.path, 'wolfram.json');
        this.ImgurConfig = path.join(pathMapper.config.path, 'imgur.json');
        this.MyApiFilmsConfig = path.join(pathMapper.config.path, 'myapifilms.json');
        this.PostgreSQLConfig = path.join(pathMapper.config.path, 'postgresql.json');

        this.DatabaseDriver = path.join(pathMapper.database.path, 'DatabaseDriver');
        this.EventHandler = path.join(pathMapper.structures.path, 'EventHandler');
        this.Interval = path.join(pathMapper.structures.path, 'Interval');
        this.MessageWatcher = path.join(pathMapper.structures.path, 'MessageWatcher');
        this.Command = path.join(pathMapper.structures.path, 'Command');
        this.ArgumentInfo = path.join(pathMapper.structures.path, 'ArgumentInfo');
        this.ArgumentType = path.join(pathMapper.structures.path, 'ArgumentType');
        this.DefaultGroups = path.join(pathMapper.client.path, 'DefaultGroups.json');
        this.StringUtil = path.join(pathMapper.modules.path, 'StringUtil');
        this.TimeUtil = path.join(pathMapper.modules.path, 'TimeUtil');
        this.DiscordFormatter = path.join(pathMapper.modules.path, 'DiscordFormatter');
        this.Logger = path.join(pathMapper.tools.path, 'Logger');
        this.TumblrModule = path.join(pathMapper.modules.path, 'TumblrModule');
        this.InstagramModule = path.join(pathMapper.modules.path, 'InstagramModule');
        this.RedditModule = path.join(pathMapper.modules.path, 'RedditModule');
        this.YoutubeModule = path.join(pathMapper.modules.path, 'YoutubeModule');
        this.GuildRegistry = path.join(pathMapper.client.path, 'GuildRegistry');
        this.UserRegistry = path.join(pathMapper.client.path, 'UserRegistry');
        this.CommandRegistry = path.join(pathMapper.client.path, 'CommandRegistry');
        this.TypeRegistry = path.join(pathMapper.client.path, 'TypeRegistry');
        this.GroupRegistry = path.join(pathMapper.client.path, 'GroupRegistry');
        this.GuildRoleGroupRegistry = path.join(pathMapper.client.path, 'GuildRoleGroupRegistry');
        this.Registry = path.join(pathMapper.client.path, 'Registry');
        this.EventLoader = path.join(pathMapper.modules.path, 'EventLoader');
        this.CommandLoader = path.join(pathMapper.modules.path, 'CommandLoader');
        this.TypeLoader = path.join(pathMapper.modules.path, 'TypeLoader');
        this.IntervalRunner = path.join(pathMapper.modules.path, 'IntervalRunner');
        this.MessageWatcherRegistry = path.join(pathMapper.modules.path, 'MessageWatcherRegistry');

        this.taylorBotClient = path.join(pathMapper.client.path, 'TaylorBotClient');

        this.eventsFolderPath = pathMapper.events.path;
        this.intervalsFolderPath = pathMapper.intervals.path;
        this.watchersFolderPath = pathMapper.watchers.path;
        this.commandsFolderPath = pathMapper.commands.path;
        this.typesFolderPath = pathMapper.types.path;
    }
}

module.exports = GlobalPaths;