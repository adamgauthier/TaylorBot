'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'tools', 'GlobalPaths'));
const globalObjects = require('globalobjects');

globalObjects.Paths = new GlobalPaths(__dirname);

const { Paths } = globalObjects;

const TaylorBotMasterClient = require(Paths.TaylorBotMasterClient);
const TimeUtil = require(Paths.TimeUtil);

const msBeforeLogin = 6 * 1000;

const main = async () => {
    const taylorbot = new TaylorBotMasterClient();
    await TimeUtil.wait(msBeforeLogin);
    await taylorbot.start();
};

main();