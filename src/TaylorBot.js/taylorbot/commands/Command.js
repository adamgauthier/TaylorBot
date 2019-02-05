'use strict';

const UserGroups = require('../client/UserGroups.js');

class Command {
    constructor({ name, aliases, group, description, minimumGroup, examples, guildOnly, guarded, args }) {
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
        this.guildOnly = guildOnly === undefined ? false : guildOnly;
        this.guarded = guarded === undefined ? false : guarded;

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
                includeNewLines: includeNewLines === undefined ? false : includeNewLines
            };
        });
    }

    run(commandContext, args) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.run.name}() method.`);
    }
}

module.exports = Command;