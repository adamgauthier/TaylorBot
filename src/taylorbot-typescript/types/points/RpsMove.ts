import TextArgumentType = require('../base/Text');
import RpsMove = require('../../modules/points/RpsMove');
import { UnsafeRandomModule } from '../../modules/random/UnsafeRandomModule';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class RpsMoveArgumentType extends TextArgumentType {
    get id(): string {
        return 'rps-move-or-random';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default(): symbol {
        return UnsafeRandomModule.randomInArray(Object.values(RpsMove));
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<symbol> {
        switch (val.trim()) {
            case 'r':
            case 'rock':
                return RpsMove.ROCK;
            case 'p':
            case 'paper':
                return RpsMove.PAPER;
            case 's':
            case 'scissors':
                return RpsMove.SCISSORS;
            default:
                return UnsafeRandomModule.randomInArray(Object.values(RpsMove));
        }
    }
}

export = RpsMoveArgumentType;
