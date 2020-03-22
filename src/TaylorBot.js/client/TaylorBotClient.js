const Discord = require('discord.js');

const EventLoader = require('../modules/EventLoader.js');
const { loginToken } = require('../config/discord.json');
const Log = require('../tools/Logger.js');
const IntervalRunner = require('../intervals/IntervalRunner.js');
const EmbedUtil = require('../modules/EmbedUtil.js');

class TaylorBotClient extends Discord.Client {
    constructor(master) {
        super({
            'disabledEvents': ['TYPING_START']
        });

        this.master = master;

        this.intervalRunner = new IntervalRunner(this);
    }

    async load() {
        Log.info('Loading events...');
        await EventLoader.loadAll(this);
        Log.info('Events loaded!');

        Log.info('Loading intervals...');
        await this.intervalRunner.loadAll();
        Log.info('Intervals loaded!');
    }

    async start() {
        await this.login(loginToken);
    }

    resolveTextBasedChannel(textBasedResolvable) {
        const tc = this.resolveTextChannel(textBasedResolvable);
        if (tc) return tc;

        const u = this.resolveUser(textBasedResolvable);
        if (u) return u;

        const dm = this.resolveDMChannel(textBasedResolvable);
        if (dm) return dm;

        return null;
    }

    resolveGuild(guildResolvable) {
        if (guildResolvable instanceof Discord.Guild) {
            return guildResolvable;
        }
        else if (guildResolvable instanceof Discord.Message || guildResolvable instanceof Discord.GuildChannel) {
            return guildResolvable.guild;
        }
        else if (typeof guildResolvable === 'string') {
            const g = this.guilds.get(guildResolvable);
            if (g) return g;

            const c = this.channels.get(guildResolvable);
            if (c) return c.guild;

            const m = this.idToMessage(guildResolvable);
            if (m && m.guild) return m.guild;
        }
        return null;
    }

    resolveTextChannel(textChannelResolvable, guildResolvable) {
        if (textChannelResolvable instanceof Discord.TextChannel) {
            return textChannelResolvable;
        }

        const guild = this.resolveGuild(guildResolvable);
        if (guild) {
            if (typeof textChannelResolvable === 'string') {
                return guild.channels.get(textChannelResolvable);
            }
            return guild.defaultChannel;
        }
        else {
            const c = this.resolveChannel(textChannelResolvable);
            if (c && c instanceof Discord.TextChannel)
                return c;
        }
        return null;
    }

    resolveDMChannel(dmChannelResolvable) {
        if (dmChannelResolvable instanceof Discord.DMChannel) {
            return dmChannelResolvable;
        }
        else {
            const dm = this.resolveChannel(dmChannelResolvable);
            if (dm && dm instanceof Discord.DMChannel) return dm;

            const u = this.resolveUser(dmChannelResolvable);
            if (u && u.dmChannel) return u.dmChannel;

            const m = this.resolveMessage(dmChannelResolvable);
            if (m && m.author.dmChannel) return m.author.dmChannel;
        }
        return null;
    }

    resolveChannel(channelResolvable) {
        if (channelResolvable instanceof Discord.Channel) {
            return channelResolvable;
        }
        else if (typeof channelResolvable === 'string') {
            const c = this.channels.get(channelResolvable);
            if (c) return c;

            const m = this.idToMessage(channelResolvable);
            if (m) return m.channel;

            const g = this.guilds.get(channelResolvable);
            if (g) return g.defaultChannel;
        }
        else if (channelResolvable instanceof Discord.Message) {
            return channelResolvable.channel;
        }
        else if (channelResolvable instanceof Discord.Guild) {
            return channelResolvable.defaultChannel;
        }
        return null;
    }

    resolveUser(userResolvable) {
        if (userResolvable instanceof Discord.User) {
            return userResolvable;
        }
        else if ((userResolvable instanceof Discord.GuildMember) || (userResolvable instanceof Discord.Client)) {
            return userResolvable.user;
        }
        else if (userResolvable instanceof Discord.Guild) {
            return userResolvable.owner.user;
        }
        else if (userResolvable instanceof Discord.Message) {
            return userResolvable.author;
        }
        else if (typeof userResolvable === 'string') {
            const u = this.users.get(userResolvable);
            if (u) return u;

            const g = this.guilds.get(userResolvable);
            if (g) return this.resolveUser(g);

            const m = this.idToMessage(userResolvable);
            if (m) return this.resolveUser(m);
        }
        return null;
    }

    resolveMessage(messageResolvable) {
        if (messageResolvable instanceof Discord.Message) {
            return messageResolvable;
        }
        else if (messageResolvable instanceof Discord.Guild) {
            return this.resolveMessage(messageResolvable.defaultChannel);
        }
        else if (typeof messageResolvable === 'string') {
            const m = this.idToMessage(messageResolvable);
            if (m) return m;
        }
        else {
            const tbc = this.resolveTextBasedChannel(messageResolvable);
            if (tbc && tbc.lastMessageID) {
                const m = this.idToMessage(tbc.lastMessageID);
                if (m) return m;
            }
        }
        return null;
    }

    idToMessage(messageId) {
        for (const c of this.channels.values()) {
            if (c.messages) {
                const m = c.messages.get(messageId);
                if (m) return m;
            }
        }
        return null;
    }

    async sendMessage(recipient, text, options) {
        const tbc = this.resolveTextBasedChannel(recipient);

        return tbc.send(
            text.replace(/(@)(everyone|here)/g, (m, p1, p2) => `${p1}\u200B${p2}`),
            options
        );
    }

    sendEmbed(recipient, embed) {
        const options = { embed };

        return this.sendMessage(recipient, '', options);
    }

    sendEmbedError(recipient, errorMessage) {
        return this.sendEmbed(recipient, EmbedUtil.error(errorMessage));
    }

    sendEmbedSuccess(recipient, successMessage) {
        return this.sendEmbed(recipient, EmbedUtil.success(successMessage));
    }
}

module.exports = TaylorBotClient;
