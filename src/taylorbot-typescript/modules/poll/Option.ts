export class Option {
    readonly name: string;
    voteCount: number;

    constructor(name: string) {
        this.name = name;
        this.voteCount = 0;
    }

    incrementVoteCount(): void {
        this.voteCount++;
    }

    decrementVoteCount(): void {
        this.voteCount--;
    }
}
