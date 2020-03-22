'use strict';

class Option {
    constructor(name) {
        this.name = name;
        this.voteCount = 0;
    }

    incrementVoteCount() {
        this.voteCount++;
    }

    decrementVoteCount() {
        this.voteCount--;
    }
}

module.exports = Option;