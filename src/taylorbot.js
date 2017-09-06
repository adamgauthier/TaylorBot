'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, 'GlobalPaths'));

const taylorbot = require(GlobalPaths.taylorBotClient);

const msBeforeLogin = 6000;

const main = async () => {
    await wait();
    await taylorbot.start();
};

const wait = () => {
    return new Promise(resolve => {
        setTimeout(resolve, msBeforeLogin);
    });
};

main();