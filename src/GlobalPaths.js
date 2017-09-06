'use strict';

const path = require('path');

const pathMapper = require(path.join(__dirname, 'PathMapper'));

class GlobalPaths {
    constructor() {
        this.EventHandler = path.join(pathMapper.structures.path, 'EventHandler');
        this.Interval = path.join(pathMapper.structures.path, 'Interval');
        this.StringUtil = path.join(pathMapper.modules.path, 'StringUtil');
        this.TumblrModule = path.join(pathMapper.modules.path, 'TumblrModule');
        this.Config = path.join(pathMapper.path, 'auth.json');

        this.pathMapper = pathMapper;
        this.intervalRunner = path.join(pathMapper.modules.path, 'IntervalRunner');
        this.eventLoader = path.join(pathMapper.modules.path, 'EventLoader');
        this.taylorBotClient = path.join(pathMapper.path, 'TaylorBotClient');
        this.databaseDriver = path.join(pathMapper.modules.database.path, 'DatabaseDriver');
    }
}

module.exports = new GlobalPaths();