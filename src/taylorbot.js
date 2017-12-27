'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'GlobalPaths'));

const taylorbot = require(GlobalPaths.taylorBotClient);
const TimeUtil = require(GlobalPaths.TimeUtil);

const msBeforeLogin = 6000;

const main = async () => {
    await TimeUtil.wait(msBeforeLogin);
    await taylorbot.start();
};

main();