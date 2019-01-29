'use strict';

process.on('unhandledRejection', reason => { throw reason; });

const TaylorBotMasterClient = require('./client/TaylorBotMasterClient.js');
const TimeUtil = require('./modules/TimeUtil.js');

const main = async () => {
    const masterClient = new TaylorBotMasterClient();
    await masterClient.load();
    await TimeUtil.waitSeconds(6);
    await masterClient.start();
};

main();