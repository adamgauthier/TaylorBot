'use strict';

const Attribute = require('./Attribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');

class UserAttribute extends Attribute {
    constructor(options) {
        super(options);
        if (new.target === UserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
        this.canSet = options.canSet !== undefined ? options.canSet : false;
        this.presentor = new options.presentor(this);
    }

    async retrieve(database, user) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.retrieve.name}() method.`);
    }

    async list(database, guild, entries) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.list.name}() method.`);
    }

    formatValue(attribute) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.formatValue.name}() method.`);
    }

    async getCommand(commandContext, user) {
        const attribute = await this.retrieve(commandContext.client.master.database, user);

        if (!attribute) {
            return DiscordEmbedFormatter
                .baseUserHeader(user)
                .setColor('#f04747')
                .setDescription(`${user.username}'s ${this.description} is not set. 🚫`);
        }
        else {
            return this.presentor.present(commandContext, user, attribute);
        }
    }
}

module.exports = UserAttribute;