'use strict';

const { GlobalPaths } = require('globalobjects');

const Interval = require(GlobalPaths.Interval);
const taylorbot = require(GlobalPaths.taylorBotClient);

const minutesToAdd = 1;
const msBeforeAdd = 1 * 60 * 1000;
const pointsReward = 1;
const minutesForReward = 8;
const msBeforeInactive = 10 * 60 * 1000;

class MinutesInterval extends Interval {
    constructor() {
        super(msBeforeAdd, async () => {
            const minimumLastSpoke = new Date().getTime() - msBeforeInactive;

            await taylorbot.database.addMinutes(minutesToAdd, minimumLastSpoke, minutesForReward, pointsReward);
        });
    }
}

module.exports = new MinutesInterval();