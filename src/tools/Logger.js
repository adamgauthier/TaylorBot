'use strict';

const { createLogger, format, transports } = require('winston');
const { combine, printf, colorize } = format;
const { Paths } = require('globalobjects');

const { minLogLevel } = require(Paths.TaylorBotConfig);
const TimeUtil = require('../modules/TimeUtil.js');

const logger = createLogger({
    level: minLogLevel,
    format: combine(
        colorize(),
        printf(info => {
            return `[${TimeUtil.formatLog()}] ${info.level}: ${info.message}`;
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