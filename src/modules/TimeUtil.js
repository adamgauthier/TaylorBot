'use strict';

const moment = require('moment');

class TimeUtil {
    static wait(msToWait) {
        return new Promise(resolve => {
            setTimeout(resolve, msToWait);
        });
    }

    static waitSeconds(secondsToWait) {
        return TimeUtil.wait(secondsToWait * 1000);
    }

    static formatFull(unixTime) {
        const m = moment.utc(unixTime, 'x');
        return `${m.format('MMMM Do, YYYY \\at HH:mm:ss.SSS')} UTC (${m.fromNow()})`;
    }

    static formatSmall(unixTime) {
        const m = moment.utc(unixTime, 'x');
        return m.format('MMMM Do, YYYY');
    }

    static formatMini(unixTime) {
        const m = moment.utc(unixTime, 'x');
        return m.format('MMM Do, YYYY');
    }

    static formatLog(unixTime = null) {
        const m = unixTime === null ? moment() : moment(unixTime, 'x');
        return m.format('MMM Do YY, HH:mm:ss Z');
    }
}

module.exports = TimeUtil;