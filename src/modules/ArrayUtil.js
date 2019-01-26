'use strict';

class ArrayUtil {
    static *iterateArrays(...arrays) {
        const { length } = arrays[0];

        if (arrays.some(array => array.length !== length))
            throw new Error(`All arrays don't have the same length (${length}).`);

        let currentIndex = 0;
        while (currentIndex < length) {
            yield arrays.map(a => a[currentIndex]);
            currentIndex++;
        }
    }

    static chunk(array, size) {
        let index = 0;
        let resIndex = 0;
        const result = new Array(Math.ceil(array.length / size));

        while (index < array.length) {
            result[resIndex++] = array.slice(index, (index += size));
        }
        return result;
    }

    static groupBy(list, keyGetter) {
        const map = new Map();
        list.forEach(item => {
            const key = keyGetter(item);
            const collection = map.get(key);
            if (!collection) {
                map.set(key, [item]);
            } else {
                collection.push(item);
            }
        });
        return map;
    }
}

module.exports = ArrayUtil;