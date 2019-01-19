'use strict';

// Rejection sampling from http://dimitri.xyz/random-ints-from-random-bits/

const crypto = require('crypto');
const Log = require('../../tools/Logger.js');

// 32 bit maximum
const maxRange = 4294967296;  // 2^32
function getRandSample() {
    return new Promise((resolve, reject) => {
        crypto.randomBytes(4, (err, buf) => {
            if (err) reject(err);
            resolve(buf.readUInt32LE());
        });
    });
}

function unsafeCoerce(sample, range) { return sample % range; }
function inExtendedRange(sample, range) { return sample < Math.floor(maxRange / range) * range; }

/* extended range rejection sampling */
const maxIter = 100;

class RandomModule {
    static async rejectionSampling(range, inRange, coerce) {
        let sample;
        let i = 0;
        do {
            sample = await getRandSample();
            if (i >= maxIter) {
                // do some error reporting.
                Log.warn(`Took more than ${maxIter} rejection sampling iterations. Check your source of randomness.`);
                break; /* just returns biased sample using remainder */
            }
            i++;
        } while (!inRange(sample, range));
        return coerce(sample, range);
    }

    // returns random value in interval [0,range) -- excludes the upper bound
    static getRandIntLessThan(range) {
        return this.rejectionSampling(Math.ceil(range), inExtendedRange, unsafeCoerce);
    }

    // returned value is in interval [low, high] -- upper bound is included
    static async getRandIntInclusive(low, hi) {
        if (low <= hi) {
            const l = Math.ceil(low); //make also work for fractional arguments
            const h = Math.floor(hi); //there must be an integer in the interval
            return (l + await this.getRandIntLessThan(h - l + 1));
        }
    }

    static async randomInArray(array) {
        return array[await this.getRandIntLessThan(array.length)];
    }
}

module.exports = RandomModule;