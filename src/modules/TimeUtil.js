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
        const m = moment(unixTime, 'x').utc();
        return `${m.format('MMMM Do, YYYY \\at H:mm:ss.SSS')} (${m.fromNow()})`;
    }

    static formatSmall(unixTime) {
        const m = moment(unixTime, 'x').utc();
        return m.format('MMMM Do, YYYY');
    }

    static formatLog(unixTime = null) {
        const m = unixTime === null ? moment() : moment(unixTime, 'x');
        return m.format('MMM Do YY, H:mm:ss Z');
    }
}

module.exports = TimeUtil;