'use strict';

const moment = require('moment');

class TimeUtil {
    static wait(msToWait) {
        return new Promise(resolve => {
            setTimeout(resolve, msToWait);
        });
    }

    static formatFull(unixTime) {
        const m = moment(unixTime, 'x');
        return `${m.format('MMMM Do, YYYY \\at H:mm:ss.SSS')} (${m.fromNow()})`;
    }
}

module.exports = TimeUtil;