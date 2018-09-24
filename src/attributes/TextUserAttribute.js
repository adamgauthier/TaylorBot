'use strict';

const UserAttribute = require('./UserAttribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
const ArrayEmbedMemberDescriptionPageMessage = require('../modules/paging/ArrayEmbedMemberDescriptionPageMessage.js');
const ArrayUtil = require('../modules/ArrayUtil.js');

class TextUserAttribute extends UserAttribute {
    constructor(options) {
        super(options);
        if (new.target === TextUserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    async retrieve(commandContext, user) {
        const attribute = await commandContext.client.master.database.textAttributes.get(this.id, user);

        if (!attribute) {
            return DiscordEmbedFormatter
                .baseUserHeader(user)
                .setColor('#f04747')
                .setDescription(`${user.username}'s ${this.description} is not set. 🚫`);
        }
        else {
            return this.getEmbed(commandContext, user, attribute.attribute_value);
        }
    }

    getEmbed(commandContext, user, attribute) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.getEmbed.name}() method.`);
    }

    async set({ client, message }, value) {
        const { author } = message;
        const attribute = await client.master.database.textAttributes.set(this.id, author, this.valueToString(value));

        return DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor('#43b581')
            .setDescription(`Your ${this.description} has been set to '${attribute.attribute_value}'. ✅`);
    }

    valueToString(value) {
        return value.toString();
    }

    async clear({ client, message }) {
        const { author } = message;
        await client.master.database.textAttributes.clear(this.id, author);

        return DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription(`Your ${this.description} has been cleared. ✅`);
    }

    async list({ client, message }, guild) {
        const attributes = await client.master.database.textAttributes.listInGuild(this.id, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`List of ${this.description}`);

        return new ArrayEmbedMemberDescriptionPageMessage(
            client,
            message.author,
            ArrayUtil.chunk(attributes, 20),
            embed,
            guild,
            (member, { attribute_value }) => `${member.user.username} - ${this.format(attribute_value)}`
        );
    }

    format(attribute) {
        return attribute;
    }
}

module.exports = TextUserAttribute;