'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const ArrayUtil = require('../../modules/ArrayUtil.js');
const RpsMove = require('../../modules/points/RpsMove.js');

class RockPaperScissorsCommand extends Command {
    constructor() {
        super({
            name: 'rockpaperscissors',
            aliases: ['rps'],
            group: 'points',
            description: 'Play a game of rock paper scissors with the bot. If you win, you gain 1 taypoint.',
            examples: ['rock', 'paper', 'scissors'],

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

    async run({ message, client }, { move }) {
        const { author, channel } = message;

        const opponentMove = ArrayUtil.random(Object.values(RpsMove));
        const winner = this.findWinner(move, opponentMove);
        const winReward = 1;

        let color;
        let resultMessage;
        if (winner === move) {
            const [{ taypoint_count }] = await client.master.database.users.addTaypointCount([author], winReward);
            color = '#43b581';
            resultMessage = `You win! 😭 Gave you ${StringUtil.plural(winReward, 'taypoint', '**')}, you now have ${taypoint_count}. 💰`;
        }
        else if (winner === opponentMove) {
            color = '#f04747';
            resultMessage = `You lost! 😂`;
        }
        else {
            color = '#faa61a';
            resultMessage = `It's a tie! 🤔`;
        }

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor(color)
            .setDescription([
                `You picked ${this.moveToEmoji(move)}, I picked ${this.moveToEmoji(opponentMove)}.`,
                resultMessage
            ].join('\n'))
        );
    }

    findWinner(playerMove, opponentMove) {
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

    moveToEmoji(move) {
        switch (move) {
            case RpsMove.ROCK:
                return '🗿';
            case RpsMove.PAPER:
                return '📄';
            case RpsMove.SCISSORS:
                return '✂';
        }
    }
}

module.exports = RockPaperScissorsCommand;