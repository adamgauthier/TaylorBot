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
}

module.exports = ArrayUtil;