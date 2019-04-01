'use strict';

process.on('unhandledRejection', reason => { throw reason; });

const TaylorBotMasterClient = require('./client/TaylorBotMasterClient.js');
const TimeUtil = require('./modules/TimeUtil.js');

const masterClient = new TaylorBotMasterClient();

const gracefulExit = () => {
    masterClient.unload();
    process.exit(0);
};

process.on('SIGINT', gracefulExit);
process.on('SIGTERM', gracefulExit);

const main = async () => {
    await masterClient.load();
    await TimeUtil.waitSeconds(6);
    await masterClient.start();
};

main();