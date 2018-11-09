'use strict';

const Attribute = require('./Attribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
const ArrayEmbedMemberDescriptionPageMessage = require('../modules/paging/ArrayEmbedMemberDescriptionPageMessage.js');
const ArrayUtil = require('../modules/ArrayUtil.js');

class MemberAttribute extends Attribute {
    constructor(options) {
        super(options);
        if (new.target === MemberAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    async getCommand(commandContext, member) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.getCommand.name}() method.`);
    }

    async rank(database, guild, entries) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.rank.name}() method.`);
    }

    presentRankEntry(member, entry) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.presentRankEntry.name}() method.`);
    }

    async rankCommand({ message, client }, guild) {
        const members = await this.rank(client.master.database, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`Ranking of ${this.description}`);

        return new ArrayEmbedMemberDescriptionPageMessage(
            client,
            message.author,
            ArrayUtil.chunk(members, 10),
            embed,
            guild,
            this.presentRankEntry
        );
    }
}

module.exports = MemberAttribute;