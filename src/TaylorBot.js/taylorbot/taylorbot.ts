process.on('unhandledRejection', reason => { throw reason; });

import TaylorBotMasterClient = require('./client/TaylorBotMasterClient.js');
import TimeUtil = require('./modules/TimeUtil.js');

const masterClient = new TaylorBotMasterClient();

const gracefulExit = (): never => {
    masterClient.unload();
    process.exit(0);
};

process.on('SIGINT', gracefulExit);
process.on('SIGTERM', gracefulExit);

const main = async (): Promise<void> => {
    await masterClient.load();
    await TimeUtil.waitSeconds(6);
    await masterClient.start();
};

main();
