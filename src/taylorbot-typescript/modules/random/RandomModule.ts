import crypto = require('crypto');
import { Log } from '../../tools/Logger';

// Rejection sampling from http://dimitri.xyz/random-ints-from-random-bits/

// 32 bit maximum
const maxRange = 4294967296;  // 2^32
function getRandSample(): Promise<number> {
    return new Promise((resolve, reject) => {
        crypto.randomBytes(4, (err, buf) => {
            if (err) reject(err);
            resolve(buf.readUInt32LE());
        });
    });
}

function unsafeCoerce(sample: number, range: number): number { return sample % range; }
function inExtendedRange(sample: number, range: number): boolean { return sample < Math.floor(maxRange / range) * range; }

/* extended range rejection sampling */
const maxIter = 100;

export class RandomModule {
    static async rejectionSampling(range: number, inRange: (sample: number, range: number) => boolean, coerce: (sample: number, range: number) => number): Promise<number> {
        let sample: number;
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
    static getRandIntLessThan(range: number): Promise<number> {
        return this.rejectionSampling(Math.ceil(range), inExtendedRange, unsafeCoerce);
    }

    // returned value is in interval [low, high] -- upper bound is included
    static async getRandIntInclusive(low: number, hi: number): Promise<number> {
        if (low <= hi) {
            const l = Math.ceil(low); // make also work for fractional arguments
            const h = Math.floor(hi); // there must be an integer in the interval
            return (l + await this.getRandIntLessThan(h - l + 1));
        }
        throw new Error(`Invalid parameters: ${low}, ${hi}`);
    }

    static async randomInArray<T>(array: T[]): Promise<T> {
        return array[await this.getRandIntLessThan(array.length)];
    }
}
