'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'GlobalPaths'));
const globalObjects = require('globalobjects');

globalObjects.GlobalPaths = new GlobalPaths();

const taylorbot = require(globalObjects.GlobalPaths.taylorBotClient);
const TimeUtil = require(globalObjects.GlobalPaths.TimeUtil);

const msBeforeLogin = 6000;

const main = async () => {
    await TimeUtil.wait(msBeforeLogin);
    await taylorbot.start();
};

main();