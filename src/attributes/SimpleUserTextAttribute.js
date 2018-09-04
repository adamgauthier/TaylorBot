'use strict';

const UserAttribute = require('./UserAttribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');

class SimpleUserTextAttribute extends UserAttribute {
    constructor(options) {
        super(options);
        if (new.target === SimpleUserTextAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
        this.value = options.value;
    }

    async retrieve({ client }, user) {
        const attribute = await client.master.database.textAttributes.get(this.id, user);

        if (!attribute) {
            return DiscordEmbedFormatter
                .baseUserHeader(user)
                .setColor('#f04747')
                .setDescription(`${user.username}'s ${this.description} is not set. 🚫`);
        }
        else {
            return DiscordEmbedFormatter
                .baseUserEmbed(user)
                .setTitle(`${user.username}'s ${this.description}`)
                .setDescription(attribute.attribute_value);
        }
    }

    async set({ client, message }, value) {
        const { author } = message;
        const attribute = await client.master.database.textAttributes.set(this.id, author, value);

        return DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription(`Your ${this.description} has been set to '${attribute.attribute_value}'. ✅`);
    }

    async clear({ client, message }) {
        const { author } = message;
        await client.master.database.textAttributes.clear(this.id, author);

        return DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription(`Your ${this.description} has been cleared. ✅`);
    }
}

module.exports = SimpleUserTextAttribute;