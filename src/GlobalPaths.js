'use strict';

const path = require('path');

const PathMapper = require(path.join(__dirname, 'tools', 'PathMapper'));

class GlobalPaths {
    constructor() {
        const pathMapper = new PathMapper(__dirname);

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

        this.EventHandler = path.join(pathMapper.structures.path, 'EventHandler');
        this.Interval = path.join(pathMapper.structures.path, 'Interval');
        this.StringUtil = path.join(pathMapper.modules.path, 'StringUtil');
        this.TimeUtil = path.join(pathMapper.modules.path, 'TimeUtil');
        this.DiscordFormatter = path.join(pathMapper.modules.path, 'DiscordFormatter');
        this.Logger = path.join(pathMapper.tools.path, 'Logger');
        this.TumblrModule = path.join(pathMapper.modules.path, 'TumblrModule');
        this.InstagramModule = path.join(pathMapper.modules.path, 'InstagramModule');
        this.RedditModule = path.join(pathMapper.modules.path, 'RedditModule');
        this.YoutubeModule = path.join(pathMapper.modules.path, 'YoutubeModule');
        this.GuildSettings = path.join(pathMapper.modules.path, 'GuildSettings');
        this.UserSettings = path.join(pathMapper.modules.path, 'UserSettings');
        this.EventLoader = path.join(pathMapper.modules.path, 'EventLoader');
        this.IntervalRunner = path.join(pathMapper.modules.path, 'IntervalRunner');

        this.taylorBotClient = path.join(pathMapper.path, 'TaylorBotClient');
        this.databaseDriver = path.join(pathMapper.modules.database.path, 'DatabaseDriver');

        this.eventsFolderPath = pathMapper.events.path;
        this.intervalsFolderPath = pathMapper.intervals.path;
    }
}

module.exports = GlobalPaths;