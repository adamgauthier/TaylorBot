'use strict';

class ArrayUtil {
    static *iterateArrays(arr1, arr2) {
        if (arr1.length !== arr2.length)
            throw new Error(`Both arrays don't have the same length (${arr1.length} & ${arr2.length}).`);

        let currentIndex = 0;
        while (currentIndex < arr1.length) {
            yield [arr1[currentIndex], arr2[currentIndex]];
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
}

module.exports = ArrayUtil;