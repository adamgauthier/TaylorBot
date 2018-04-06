'use strict';

const Discord = require('discord.js');
const Commando = require('discord.js-commando');
const { GlobalPaths } = require('globalobjects');

const EventLoader = require(GlobalPaths.EventLoader);
const DatabaseDriver = require(GlobalPaths.DatabaseDriver);
const { loginToken } = require(GlobalPaths.DiscordConfig);
const { masterId } = require(GlobalPaths.TaylorBotConfig);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const IntervalRunner = require(GlobalPaths.IntervalRunner);
const Registry = require(GlobalPaths.Registry);

const discordMax = 2000;

class TaylorBotClient extends Commando.Client {
    constructor(options) {
        super(options);
        this.database = new DatabaseDriver();
        this.intervalRunner = new IntervalRunner(this);
        this.eventLoader = new EventLoader();
        this.oldRegistry = new Registry(this);
    }

    async start() {
        Log.info('Loading database...');
        await this.database.load();
        Log.info('Database loaded!');

        Log.info('Loading intervals...');
        this.intervalRunner.loadAll();
        Log.info('Intervals loaded!');

        Log.info('Loading events...');
        await this.eventLoader.loadAll(this);
        Log.info('Events loaded!');

        await this.oldRegistry.loadAll();

        await this.login(loginToken);
    }

    resolveTextBasedChannel(textBasedResolvable) {
        const tc = this.resolveTextChannel(textBasedResolvable);
        if (tc) return tc;

        const u = this.resolveUser(textBasedResolvable);
        if (u) return u;

        const dm = this.resolveDMChannel(textBasedResolvable);
        if (dm) return dm;

        //TODO:add group dm

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
        else if (userResolvable instanceof Discord.GuildMember || userResolvable instanceof Discord.Client) {
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
            return this.resolveMessage(messageResolvable.defaultChannel)
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

    replyTo(msg, text, options) {
        msg = this.resolveMessage(msg);

        if (!(msg.channel instanceof Discord.DMChannel))
            text = `${msg.author.toString()} ${text}`;

        return this.sendMessage(msg.channel, text, options);
    }

    async sendMessage(recipient, text, options) {
        const tbc = this.resolveTextBasedChannel(recipient);
        try {
            return await this._sendMessage(tbc, text, options);
        }
        catch (e) {
            Log.error(`Sending message error in recipient ${recipient} : ${e}`);
        }
    }

    async _sendMessage(recipient, text, options) {
        const format = options ? options.format : undefined;

        text = text.replace(/(@)(everyone|here)/g, (m, p1, p2) => `${p1}\u200B${p2}`);

        const textEvalLength = text.length + (format ? format.after.length + format.before.length : 0);
        let firstPart = text;
        let currentCallback;

        if (textEvalLength >= discordMax) {
            const max = format ? discordMax - format.after.length - format.before.length : discordMax;
            firstPart = firstPart.substring(0, max);
            let lastIndex = firstPart.lastIndexOf('\n');
            if (lastIndex === -1) lastIndex = firstPart.lastIndexOf('.');
            if (lastIndex === -1) lastIndex = firstPart.lastIndexOf(' ');
            if (lastIndex === -1) lastIndex = max - 1;

            lastIndex += 1;
            firstPart = text.substring(0, lastIndex);
            const lastPart = text.substring(lastIndex);

            currentCallback = msg => {
                return this._sendMessage(recipient, lastPart, options);
            };
        }
        else {
            currentCallback = msg => {
                return Promise.resolve(msg);
            };
        }

        if (format) firstPart = format.before + firstPart + format.after;

        try {
            const msg = await recipient.send(firstPart, options);
            return await currentCallback(msg);
        }
        catch (e) {
            return Promise.reject(`${e} when trying to send message.`);
        }
    }

    async sendEmbed(recipient, embed) {
        const options = { 'embed': embed };

        return await this.sendMessage(recipient, '', options);
    }
}

const taylorbot = new TaylorBotClient({
    'fetchAllMembers': true,
    'disabledEvents': ['TYPING_START'],
    'owner': masterId,
    'commandPrefix': '!'
});

taylorbot.registry
    .registerDefaultGroups()
    .registerDefaultTypes()
    .registerDefaultCommands({
        'ping': false
    })
    .registerCommandsIn(GlobalPaths.commandsFolderPath);

module.exports = taylorbot;