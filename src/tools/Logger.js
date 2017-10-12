'use strict';

const moment = require('moment');
const { createLogger, format, transports } = require('winston');
const { combine, printf, colorize } = format;

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const { minLogLevel } = require(GlobalPaths.Config);

const logger = createLogger({
    level: minLogLevel,
    format: combine(
        colorize(),
        printf(info => {
            return `[${moment().format('MMM Do YY, H:mm:ss Z')}] ${info.level}: ${info.message}`;
        })
    ),
    transports: [new transports.Console()]
});

class Logger {
    static error(message) {
        logger.error(message);
    }

    static warn(message) {
        logger.warn(message);
    }

    static info(message) {
        logger.info(message);
    }

    static verbose(message) {
        logger.verbose(message);
    }

    static debug(message) {
        logger.debug(message);
    }

    static silly(message) {
        logger.silly(message);
    }
}

module.exports = Logger;