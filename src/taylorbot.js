'use strict';

process.on('unhandledRejection', reason => { throw reason; });

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'tools', 'GlobalPaths'));
const globalObjects = require('globalobjects');

globalObjects.Paths = new GlobalPaths(__dirname);

const TaylorBotMasterClient = require('./client/TaylorBotMasterClient.js');
const TimeUtil = require('./modules/TimeUtil.js');

const main = async () => {
    const masterClient = new TaylorBotMasterClient();
    await masterClient.load();
    await TimeUtil.waitSeconds(6);
    await masterClient.start();
};

main();