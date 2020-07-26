import ArgumentType = require('../ArgumentType');

class AnyTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: true,
            mustBeQuoted: false
        });
    }

    get id(): string {
        return 'any-text';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default(): string {
        return '';
    }

    async parse(val: string): Promise<string> {
        return val;
    }
}

export = AnyTextArgumentType;
