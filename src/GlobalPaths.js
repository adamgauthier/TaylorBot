'use strict';

const path = require('path');

const PathMapper = require(path.join(__dirname, 'tools', 'PathMapper'));

class GlobalPaths {
    constructor() {
        const pathMapper = new PathMapper(__dirname);
        this.pathMapper = pathMapper;

        // Config
        this.Config = path.join(pathMapper.path, 'auth.json');
        this.DiscordConfig = path.join(pathMapper.config.path, 'discord.json');
        this.TumblrConfig = path.join(pathMapper.config.path, 'tumblr.json');

        this.EventHandler = path.join(pathMapper.structures.path, 'EventHandler');
        this.Interval = path.join(pathMapper.structures.path, 'Interval');
        this.StringUtil = path.join(pathMapper.modules.path, 'StringUtil');
        this.Logger = path.join(pathMapper.tools.path, 'Logger');
        this.TumblrModule = path.join(pathMapper.modules.path, 'TumblrModule');

        this.intervalRunner = path.join(pathMapper.modules.path, 'IntervalRunner');
        this.eventLoader = path.join(pathMapper.modules.path, 'EventLoader');
        this.taylorBotClient = path.join(pathMapper.path, 'TaylorBotClient');
        this.databaseDriver = path.join(pathMapper.modules.database.path, 'DatabaseDriver');
    }
}

module.exports = new GlobalPaths();