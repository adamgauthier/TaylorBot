'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class Ready extends EventHandler {
    constructor() {
        super(Events.READY);
    }

    async handler(client) {
        Log.info('Client is ready!');

        client.intervalRunner.startAll();
        Log.info('Intervals started!');

        Log.info('Checking new guilds, users and usernames...');
        await this.syncDatabase(client);
        Log.info('New guilds, users and usernames checked!');
    }

    async syncDatabase(client) {
        const { registry, database } = client.master;

        const startupTime = Date.now();
        const allGuildMembers = await database.guildMembers.getAll();
        const channels = await database.textChannels.getAll();
        const latestGuildNames = await database.guildNames.getAllLatest();
        const latestUsernames = new Map(
            (await database.usernames.getAllLatest())
                .map(({ user_id, username }) => [user_id, username])
        );

        for (const guild of client.guilds.values()) {
            if (!registry.guilds.has(guild.id)) {
                Log.warn(`Found new guild ${Format.guild(guild)}.`);
                await registry.guilds.addGuild(guild, startupTime);
            }
            else {
                const latestGuildName = latestGuildNames.find(gn => gn.guild_id === guild.id);
                if (!latestGuildName || guild.name !== latestGuildName.guild_name) {
                    await database.guildNames.add(guild, startupTime);
                    Log.warn(`Added new guild name for ${Format.guild(guild)}.${latestGuildName ? ` Old guild name was ${latestGuildName.guild_name}.` : ''}`);
                }
            }

            for (const textChannel of guild.channels.filter(c => c.type === 'text').values()) {
                if (!channels.some(c => c.channel_id === textChannel.id)) {
                    Log.warn(`Found new text channel ${Format.channel(textChannel)}.`);
                    await database.textChannels.add(textChannel, startupTime);
                }
            }

            const guildMembers = allGuildMembers
                .filter(gm => gm.guild_id === guild.id)
                .map(gm => { gm.nowAlive = false; return gm; });
            const members = await guild.members.fetch();

            for (const member of members.values()) {
                const { user } = member;
                if (!registry.users.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await registry.users.addUser(member, startupTime);
                    latestUsernames.set(user.id, user.username);
                }
                else {
                    const guildMember = guildMembers.find(gm => gm.user_id === member.id);
                    if (!guildMember) {
                        Log.warn(`Found new member ${Format.member(member)}.`);
                        await database.guildMembers.add(member);
                    }
                    else {
                        guildMember.nowAlive = true;
                        if (guildMember.first_joined_at === '9223372036854775807') {
                            await database.guildMembers.fixInvalidJoinDate(member);
                        }
                    }

                    const latestUsername = latestUsernames.get(user.id);
                    if (!latestUsername || latestUsername !== user.username) {
                        Log.warn(`Found new username for ${Format.user(user)}.`);
                        await database.usernames.add(user, startupTime);
                        latestUsernames.set(user.id, user.username);
                    }
                }
            }

            const aliveChanged = guildMembers.filter(gm => gm.alive !== gm.nowAlive).map(gm => gm.user_id);
            if (aliveChanged.length > 0) {
                await database.guildMembers.reverseAliveInGuild(guild, aliveChanged);
            }
        }
    }
}

module.exports = Ready;