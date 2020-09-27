process.on('unhandledRejection', reason => { throw reason; });

import { TaylorBotMasterClient } from './client/TaylorBotMasterClient';
import { TimeUtil } from './modules/util/TimeUtil';

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
