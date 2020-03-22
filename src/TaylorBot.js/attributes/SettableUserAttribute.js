'use strict';

const UserAttribute = require('./UserAttribute.js');
const DiscordFormatter = require('../modules/DiscordFormatter.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
const PageMessage = require('../modules/paging/PageMessage.js');
const MemberEmbedDescriptionPageMessage = require('../modules/paging/editors/MemberEmbedDescriptionPageMessage.js');
const ArrayUtil = require('../modules/ArrayUtil.js');

class SettableUserAttribute extends UserAttribute {
    constructor(options) {
        options.canSet = true;
        super(options);
        if (new.target === SettableUserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
        this.value = options.value;
    }

    async retrieve(database, user) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.retrieve.name}() method.`);
    }

    async set(database, user, value) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.set.name}() method.`);
    }

    async clear(database, user) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.clear.name}() method.`);
    }

    async list(database, guild, entries) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.list.name}() method.`);
    }

    formatValue(attribute) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.formatValue.name}() method.`);
    }

    async setCommand({ message, client }, value) {
        const { author } = message;
        const attribute = await this.set(client.master.database, author, value);

        return DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor('#43b581')
            .setDescription(`Your ${this.description} has been set to '${this.formatValue(attribute)}'. ✅`);
    }

    async clearCommand({ message, client }) {
        const { author } = message;
        await this.clear(client.master.database, author);

        return DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription(`Your ${this.description} has been cleared. ✅`);
    }

    async listCommand({ client, message }, guild) {
        const attributes = await this.list(client.master.database, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`List of ${this.description}`);

        return new PageMessage(
            client,
            message.author,
            ArrayUtil.chunk(attributes, 20),
            new MemberEmbedDescriptionPageMessage(
                embed,
                client.master.database,
                guild,
                (member, attribute) => `${DiscordFormatter.escapeDiscordMarkdown(member.user.username)} - ${this.formatValue(attribute)}`
            )
        );
    }
}

module.exports = SettableUserAttribute;
