'use strict';

const TextArgumentType = require('../base/Text.js');
const RpsMove = require('../../modules/points/RpsMove.js');
const UnsafeRandomModule = require('../../modules/random/UnsafeRandomModule.js');

class RpsMoveArgumentType extends TextArgumentType {
    get id() {
        return 'rps-move-or-random';
    }

    canBeEmpty() {
        return true;
    }

    default() {
        return UnsafeRandomModule.randomInArray(Object.values(RpsMove));
    }

    async parse(val) {
        switch (val.trim()) {
            case 'r':
            case 'rock':
                return RpsMove.ROCK;
            case 'p':
            case 'paper':
                return RpsMove.PAPER;
            case 's':
            case 'scissors':
                return RpsMove.SCISSORS;
            default:
                return UnsafeRandomModule.randomInArray(Object.values(RpsMove));
        }
    }
}

module.exports = RpsMoveArgumentType;