'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);

class Command {
    constructor({ name, aliases, group, description, minimumGroup, guildOnly, args }) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (!name) {
            throw new Error(`All commands must have a name. (${this.constructor.name})`);
        }

        this.name = name;
        this.aliases = aliases === undefined ? [] : aliases;
        this.group = group;
        this.description = description;
        this.minimumGroup = minimumGroup === undefined ? UserGroups.Everyone : minimumGroup;
        this.guildOnly = guildOnly;

        this.args = args.map(({ key, label, type, prompt, quoted, includeNewLines }) => {
            if (!key) {
                throw new Error(`All arguments must have a key. (Command '${name}')`);
            }

            return {
                key,
                label,
                type,
                prompt,
                quoted: quoted === undefined ? false : quoted,
                includeNewLines: includeNewLines === undefined ? false : includeNewLines
            };
        });
    }

    run(commandMessage, args) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a run() method.`);
    }
}

module.exports = Command;