'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'tools', 'GlobalPaths'));
const globalObjects = require('globalobjects');

globalObjects.Paths = new GlobalPaths(__dirname);

const { Paths } = globalObjects;

const TaylorBotMasterClient = require(Paths.TaylorBotMasterClient);
const TimeUtil = require(Paths.TimeUtil);

const main = async () => {
    const masterClient = new TaylorBotMasterClient();
    await TimeUtil.waitSeconds(6);
    await masterClient.start();
};

main();