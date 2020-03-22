'use strict';

const Attribute = require('./Attribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
const PageMessage = require('../modules/paging/PageMessage.js');
const MemberEmbedDescriptionPageMessage = require('../modules/paging/editors/MemberEmbedDescriptionPageMessage.js');
const ArrayUtil = require('../modules/ArrayUtil.js');

class MemberAttribute extends Attribute {
    constructor(options) {
        super(options);
        if (new.target === MemberAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
        this.presentor = new options.presentor(this);
        this.columnName = options.columnName;
    }

    async retrieve(database, member) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.retrieve.name}() method.`);
    }

    async getCommand(commandContext, member) {
        const attribute = await this.retrieve(commandContext.client.master.database, member);

        if (!attribute) {
            return DiscordEmbedFormatter
                .baseUserHeader(member.user)
                .setColor('#f04747')
                .setDescription(`${member.displayName}'s ${this.description} doesn't exist. 🚫`);
        }
        else {
            return this.presentor.present(commandContext, member, attribute);
        }
    }

    async rank(database, guild, entries) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.rank.name}() method.`);
    }

    async rankCommand({ message, client }, guild) {
        const members = await this.rank(client.master.database, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`Ranking of ${this.description}`);

        return new PageMessage(
            client,
            message.author,
            ArrayUtil.chunk(members, 10),
            new MemberEmbedDescriptionPageMessage(
                embed,
                client.master.database,
                guild,
                (member, attribute) => this.presentor.presentRankEntry(member, attribute)
            )
        );
    }
}

module.exports = MemberAttribute;
