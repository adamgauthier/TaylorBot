export class ArrayUtil {
    static *iterateArrays<T, U>(firstArray: T[], secondArray: U[]): IterableIterator<[T, U]> {
        if (firstArray.length !== secondArray.length)
            throw new Error(`All arrays don't have the same length (${firstArray.length}).`);

        let currentIndex = 0;
        while (currentIndex < firstArray.length) {
            yield [firstArray[currentIndex], secondArray[currentIndex]];
            currentIndex++;
        }
    }

    static chunk<T>(array: T[], size: number): T[][] {
        let index = 0;
        let resIndex = 0;
        const result = new Array(Math.ceil(array.length / size));

        while (index < array.length) {
            result[resIndex++] = array.slice(index, (index += size));
        }
        return result;
    }

    static groupBy<T, U>(list: Array<T>, keyGetter: (item: T) => U): Map<U, T[]> {
        const map = new Map<U, T[]>();
        list.forEach(item => {
            const key = keyGetter(item);
            const collection = map.get(key);
            if (collection === undefined) {
                map.set(key, [item]);
            } else {
                collection.push(item);
            }
        });
        return map;
    }
}
