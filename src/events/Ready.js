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
        const guildMembers = await database.guildMembers.getAll();
        const channels = await database.textChannels.getAll();
        const latestGuildNames = await database.guildNames.getAllLatest();
        let latestUsernames = await database.usernames.getAllLatest();

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

            const members = await guild.members.fetch();
            for (const member of members.values()) {
                const { user } = member;
                if (!registry.users.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await registry.users.addUser(member, startupTime);
                }
                else {
                    const guildMember = guildMembers.find(gm => gm.guild_id === guild.id && gm.user_id === member.id);
                    if (!guildMember) {
                        Log.warn(`Found new member ${Format.member(member)}.`);
                        await database.guildMembers.add(member);
                    }
                    else {
                        if (guildMember.first_joined_at === '9223372036854775807') {
                            await database.guildMembers.fixInvalidJoinDate(member);
                        }
                    }

                    const latestUsername = latestUsernames.find(u => u.user_id === user.id);
                    if (!latestUsername || latestUsername.username !== user.username) {
                        Log.warn(`Found new username for ${Format.user(user)}.`);
                        await database.usernames.add(user, startupTime);
                        latestUsernames = latestUsernames.filter(u => u.user_id !== user.id);
                        latestUsernames.push({ 'user_id': user.id, 'username': user.username });
                    }
                }
            }
        }
    }
}

module.exports = Ready;