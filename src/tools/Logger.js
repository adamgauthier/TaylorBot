'use strict';

const chalk = require('chalk');

const TimeUtil = require('../modules/TimeUtil.js');

class Logger {
    static _log(level, message) {
        console.log(`[${TimeUtil.formatLog()}] ${level.style(level.name)}: ${message}`);
    }

    static error(message) {
        Logger._log({ name: 'error', style: chalk.red }, message);
    }

    static warn(message) {
        Logger._log({ name: 'warn', style: chalk.yellow }, message);
    }

    static info(message) {
        Logger._log({ name: 'info', style: chalk.green }, message);
    }

    static verbose(message) {
        Logger._log({ name: 'verbose', style: chalk.cyan }, message);
    }

    static debug(message) {
        Logger._log({ name: 'debug', style: chalk.gray }, message);
    }

    static silly(message) {
        Logger._log({ name: 'silly', style: chalk.magenta }, message);
    }
}

module.exports = Logger;