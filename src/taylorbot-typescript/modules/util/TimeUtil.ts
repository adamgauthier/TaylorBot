import moment = require('moment');

export class TimeUtil {
    static wait(msToWait: number): Promise<void> {
        return new Promise(resolve => {
            setTimeout(resolve, msToWait);
        });
    }

    static waitSeconds(secondsToWait: number): Promise<void> {
        return TimeUtil.wait(secondsToWait * 1000);
    }

    static waitMinutes(minutesToWait: number): Promise<void> {
        return TimeUtil.waitSeconds(minutesToWait * 60);
    }

    static formatFull(unixTime: number): string {
        const m = moment.utc(unixTime, 'x');
        return `${m.format('MMMM Do, YYYY \\at HH:mm:ss.SSS')} UTC (${m.fromNow()})`;
    }

    static formatSmall(unixTime: number): string {
        const m = moment.utc(unixTime, 'x');
        return m.format('MMMM Do, YYYY');
    }

    static formatMini(unixTime: number): string {
        const m = moment.utc(unixTime, 'x');
        return m.format('MMM Do, YYYY');
    }

    static formatLog(unixTime: number | null = null): string {
        const m = unixTime === null ? moment() : moment(unixTime, 'x');
        return m.format('MMM Do YY, HH:mm:ss Z');
    }
}
