export class TaypointAmount {
    readonly count: number | undefined;
    readonly divisor: number | undefined;

    constructor({ count = undefined, divisor = undefined }: { count?: number; divisor?: number }) {
        if ((count !== undefined && divisor !== undefined) || (count === undefined && divisor === undefined))
            throw new Error('You must provide either a count or a divisor.');

        this.count = count;
        this.divisor = divisor;
    }

    get isRelative(): boolean {
        return this.divisor !== undefined;
    }

    toString(): string {
        return `(${this.isRelative ? `divisor: ${this.divisor}` : `count: ${this.count}`})`;
    }
}
