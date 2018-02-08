'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'tools', 'GlobalPaths'));
const globalObjects = require('globalobjects');

globalObjects.GlobalPaths = new GlobalPaths(__dirname);

const taylorbot = require(globalObjects.GlobalPaths.taylorBotClient);
const TimeUtil = require(globalObjects.GlobalPaths.TimeUtil);

const msBeforeLogin = 6000;

const main = async () => {
    await TimeUtil.wait(msBeforeLogin);
    await taylorbot.start();
};

main();