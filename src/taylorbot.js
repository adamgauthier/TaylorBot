'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'tools', 'GlobalPaths'));
const globalObjects = require('globalobjects');

globalObjects.GlobalPaths = new GlobalPaths(__dirname);

const TaylorBotMasterClient = require(globalObjects.GlobalPaths.TaylorBotMasterClient);
const TimeUtil = require(globalObjects.GlobalPaths.TimeUtil);

const msBeforeLogin = 6 * 1000;

const main = async () => {
    const taylorbot = new TaylorBotMasterClient();
    await TimeUtil.wait(msBeforeLogin);
    await taylorbot.start();
};

main();