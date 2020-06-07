import UnsafeRandomModule = require('../modules/random/UnsafeRandomModule.js');
import { CachedCommand } from '../client/registry/CachedCommand';
import { TaylorBotClient } from '../client/TaylorBotClient';
import ArgumentType = require('../types/ArgumentType.js');
import { Message, User } from 'discord.js';
import { MessageContext } from '../structures/MessageContext';

type ArgumentInfo = {
    key: string;
    label: string;
    type: string;
    prompt: string;
    mustBeQuoted: boolean;
    includesSpaces: boolean;
    includesNewLines: boolean;
};

export type CommandArgumentInfo = {
    info: ArgumentInfo;
    mustBeQuoted: boolean;
    includesSpaces: boolean;
    includesNewLines: boolean;
    type: ArgumentType;
    canBeEmpty: boolean;
};

export class CommandMessageContext {
    readonly args: CommandArgumentInfo[];
    constructor(public readonly messageContext: MessageContext, public readonly command: CachedCommand) {
        this.args = this.command.command.args.map((info: ArgumentInfo) => {
            const type: ArgumentType = this.client.master.registry.types.getType(info.type);
            const canBeEmpty = type.canBeEmpty(messageContext, info);

            return {
                info,
                mustBeQuoted: info.mustBeQuoted || type.mustBeQuoted,
                includesSpaces: info.includesSpaces || type.includesSpaces,
                includesNewLines: info.includesNewLines || type.includesNewLines,
                type,
                canBeEmpty
            };
        });
    }

    get client(): TaylorBotClient {
        return this.messageContext.client;
    }

    get author(): User {
        return this.messageContext.author;
    }

    get message(): Message {
        return this.messageContext.message;
    }

    usage(): string {
        const keyword = `${this.messageContext.prefix}${this.command.name}`;

        const args = this.args.map(({ info, canBeEmpty, mustBeQuoted }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            return mustBeQuoted ? `'<${label}>'` : `<${label}>`;
        });

        return [keyword, ...args].join(' ');
    }

    argsUsage(): { identifier: string; hint: string }[] {
        return this.args.map(({ info, canBeEmpty, mustBeQuoted }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            let hint = info.prompt;
            if (mustBeQuoted)
                hint += ' It must be surrounded by quotes!';
            if (canBeEmpty)
                hint += ' It can be empty!';

            return {
                identifier: mustBeQuoted ? `'<${label}>'` : `<${label}>`,
                hint
            };
        });
    }

    helpUsage(): string {
        const helpCommandName = 'help';
        const keyword = `${this.messageContext.prefix}${helpCommandName}`;

        return `${keyword} ${this.command.name}`;
    }

    example(): string {
        return `${this.messageContext.prefix}${this.command.command.name} ${UnsafeRandomModule.randomInArray(this.command.command.examples)}`.trimEnd();
    }
}
