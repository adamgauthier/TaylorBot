'use strict';

class TimeUtil {
    static wait(msToWait) {
        return new Promise(resolve => {
            setTimeout(resolve, msToWait);
        });
    }
}

module.exports = TimeUtil;