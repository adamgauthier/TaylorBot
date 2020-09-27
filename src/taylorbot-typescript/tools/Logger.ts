import chalk, { Chalk, ColorSupport } from 'chalk';

import { TimeUtil } from '../modules/util/TimeUtil';

export class Log {
    static _log(level: { name: string; style: Chalk & { supportsColor: ColorSupport } }, message: string): void {
        console.log(`[${TimeUtil.formatLog()}] ${level.style(level.name)}: ${message}`);
    }

    static error(message: string): void {
        Log._log({ name: 'error', style: chalk.red }, message);
    }

    static warn(message: string): void {
        Log._log({ name: 'warn', style: chalk.yellow }, message);
    }

    static info(message: string): void {
        Log._log({ name: 'info', style: chalk.green }, message);
    }

    static verbose(message: string): void {
        Log._log({ name: 'verbose', style: chalk.cyan }, message);
    }

    static debug(message: string): void {
        Log._log({ name: 'debug', style: chalk.gray }, message);
    }

    static silly(message: string): void {
        Log._log({ name: 'silly', style: chalk.magenta }, message);
    }
}
