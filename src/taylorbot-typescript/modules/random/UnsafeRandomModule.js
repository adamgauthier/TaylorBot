'use strict';

class UnsafeRandomModule {
    // min: inclusive, max: inclusive
    static getRandomInt(min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    static randomInArray(array) {
        return array[Math.floor(Math.random() * array.length)];
    }
}

module.exports = UnsafeRandomModule;