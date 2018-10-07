'use strict';

const Attribute = require('./Attribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
const ArrayEmbedMemberDescriptionPageMessage = require('../modules/paging/ArrayEmbedMemberDescriptionPageMessage.js');
const ArrayUtil = require('../modules/ArrayUtil.js');

class UserAttribute extends Attribute {
    constructor(options) {
        super(options);
        if (new.target === UserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
        this.value = options.value;
        this.presentor = new options.presentor(this);
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

        return new ArrayEmbedMemberDescriptionPageMessage(
            client,
            message.author,
            ArrayUtil.chunk(attributes, 20),
            embed,
            guild,
            (member, attribute) => `${member.user.username} - ${this.formatValue(attribute)}`
        );
    }
}

module.exports = UserAttribute;