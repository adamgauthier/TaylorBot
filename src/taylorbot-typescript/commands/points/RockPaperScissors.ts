import { Command } from '../Command';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { RandomModule } from '../../modules/random/RandomModule';
import RpsMove = require('../../modules/points/RpsMove');
import { CommandMessageContext } from '../CommandMessageContext';

class RockPaperScissorsCommand extends Command {
    constructor() {
        super({
            name: 'rockpaperscissors',
            aliases: ['rps'],
            group: 'Points 💰',
            description: 'Play a game of rock paper scissors with the bot. If you win, you gain 1 taypoint.',
            examples: ['rock', 'paper', 'scissors'],
            maxDailyUseCount: 6000,

            args: [
                {
                    key: 'move',
                    label: 'move',
                    type: 'rps-move-or-random',
                    prompt: 'What move do you want to pick (rock, paper or scissors)?'
                }
            ]
        });
    }

    async run({ message, client, author }: CommandMessageContext, { move }: { move: symbol }): Promise<void> {
        const { channel } = message;

        const opponentMove = await RandomModule.randomInArray(Object.values(RpsMove));
        const winner = this.findWinner(move, opponentMove);
        const winReward = 1;

        let color;
        let resultMessage;
        if (winner === move) {
            const { taypoint_count } = await client.master.database.rpsStats.winRpsGame(author, winReward);
            color = '#43b581';
            resultMessage = `You win! 😭 Gave you ${StringUtil.plural(winReward, 'taypoint', '**')}, you now have ${StringUtil.formatNumberString(taypoint_count)}. 💰`;
        }
        else if (winner === opponentMove) {
            color = '#f04747';
            resultMessage = `You lost! 😂`;
        }
        else {
            color = '#faa61a';
            resultMessage = `It's a tie! 🤔`;
        }

        await client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor(color)
            .setDescription([
                `You picked ${this.moveToEmoji(move)}, I picked ${this.moveToEmoji(opponentMove)}.`,
                resultMessage
            ].join('\n'))
        );
    }

    findWinner(playerMove: symbol, opponentMove: symbol): symbol | null | undefined {
        if (playerMove === opponentMove)
            return null;

        if ([playerMove, opponentMove].includes(RpsMove.ROCK) &&
            [playerMove, opponentMove].includes(RpsMove.PAPER)) {
            return RpsMove.PAPER;
        }
        if ([playerMove, opponentMove].includes(RpsMove.PAPER) &&
            [playerMove, opponentMove].includes(RpsMove.SCISSORS)) {
            return RpsMove.SCISSORS;
        }
        if ([playerMove, opponentMove].includes(RpsMove.ROCK) &&
            [playerMove, opponentMove].includes(RpsMove.SCISSORS)) {
            return RpsMove.ROCK;
        }
    }

    moveToEmoji(move: symbol): string {
        switch (move) {
            case RpsMove.ROCK:
                return '🗿';
            case RpsMove.PAPER:
                return '📄';
            case RpsMove.SCISSORS:
                return '✂';
            default:
                throw new Error(`No emoji for move ${move.toString()}.`);
        }
    }
}

export = RockPaperScissorsCommand;
