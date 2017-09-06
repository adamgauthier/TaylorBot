'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);

const minutesToAdd = 1,
      msBeforeAdd = 60000,
      pointsToAdd = 1,
      minutesForReward = 8,
      msAfterLastSpoke = 600000;

class MinutesInterval extends Interval {
    constructor() {
        super(msBeforeAdd, () => {
            database.updateMinutes(minutesToAdd, new Date().getTime() - msAfterLastSpoke, minutesForReward, pointsToAdd);
        });
    }
}

module.exports = new MinutesInterval();