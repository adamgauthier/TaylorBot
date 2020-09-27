import UserGroups = require('../client/UserGroups');
import { CommandMessageContext } from './CommandMessageContext';

export abstract class Command {
    readonly name: string;
    readonly aliases: string[];
    readonly group: string;
    readonly description: string;
    readonly minimumGroup: { name: string; accessLevel: number; isSpecial: boolean };
    readonly examples: string[];
    readonly maxDailyUseCount: number | null;
    readonly guildOnly: boolean;
    readonly proOnly: boolean;
    readonly args: { key: string; label: string; type: string; prompt: string; mustBeQuoted: boolean; includesSpaces: boolean; includesNewLines: boolean }[];

    constructor({
        name, aliases = undefined, group, description, minimumGroup = undefined,
        examples, maxDailyUseCount = undefined, guildOnly = undefined,
        proOnly = undefined, args
    }: {
        name: string;
        aliases?: string[];
        group: string;
        description: string;
        minimumGroup?: {
            name: string;
            accessLevel: number;
            isSpecial: boolean;
        };
        examples: string[];
        maxDailyUseCount?: number;
        guildOnly?: boolean;
        proOnly?: boolean;
        args: { key: string; label: string; type: string; prompt: string; mustBeQuoted?: boolean; includesSpaces?: boolean; includeNewLines?: boolean }[];
    }) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (!name) {
            throw new Error(`All commands must have a name. (${this.constructor.name})`);
        }

        if (!examples || examples.length === 0) {
            throw new Error(`All commands must have examples. (${this.constructor.name})`);
        }

        this.name = name;
        this.aliases = aliases === undefined ? [] : aliases;
        this.group = group;
        this.description = description;
        this.minimumGroup = minimumGroup === undefined ? UserGroups.Everyone : minimumGroup;
        this.examples = examples;
        this.maxDailyUseCount = maxDailyUseCount === undefined ? null : maxDailyUseCount;
        this.guildOnly = guildOnly === undefined ? false : guildOnly;
        this.proOnly = proOnly === undefined ? false : proOnly;

        this.args = args.map(({ key, label, type, prompt, mustBeQuoted, includesSpaces, includeNewLines }) => {
            if (!key) {
                throw new Error(`All arguments must have a key. (Command '${name}')`);
            }

            return {
                key,
                label,
                type,
                prompt,
                mustBeQuoted: mustBeQuoted === undefined ? false : mustBeQuoted,
                includesSpaces: includesSpaces === undefined ? false : includesSpaces,
                includesNewLines: includeNewLines === undefined ? false : includeNewLines
            };
        });
    }

    abstract run(commandContext: CommandMessageContext, args: Record<string, any>): Promise<void>;
}
