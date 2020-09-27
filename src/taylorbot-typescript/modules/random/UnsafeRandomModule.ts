export class UnsafeRandomModule {
    // min: inclusive, max: inclusive
    static getRandomInt(min: number, max: number): number {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    static randomInArray<T>(array: T[]): T {
        return array[Math.floor(Math.random() * array.length)];
    }
}
