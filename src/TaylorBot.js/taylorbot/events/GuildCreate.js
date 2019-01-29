'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class GuildCreate extends EventHandler {
    constructor() {
        super(Events.GUILD_CREATE);
    }

    async handler(client, guild) {
        Log.info(`Joined guild ${Format.guild(guild)}.`);

        const { registry, database } = client.master;

        const members = await guild.members.fetch();

        if (!registry.guilds.has(guild.id)) {
            Log.info(`Adding new guild ${Format.guild(guild)}.`);
            await registry.guilds.addGuild(guild);
        }
        else {
            const latestGuildName = await database.guildNames.getLatest(guild);
            if (!latestGuildName || guild.name !== latestGuildName.guild_name) {
                await database.guildNames.add(guild);
                Log.info(`Added new guild name for ${Format.guild(guild)}.${latestGuildName ? ` Old guild name was ${latestGuildName.guild_name}.` : ''}`);
            }
        }

        const channels = await database.textChannels.getAllInGuild(guild);
        for (const textChannel of guild.channels.filter(c => c.type === 'text').values()) {
            if (!channels.some(c => c.channel_id === textChannel.id)) {
                Log.info(`Found new text channel ${Format.channel(textChannel)}.`);
                await database.textChannels.add(textChannel);
            }
        }

        const guildMembers = (await database.guildMembers.getAllInGuild(guild))
            .map(gm => { gm.nowAlive = false; return gm; });
        let latestUsernames = await database.usernames.getAllLatest();

        for (const member of members.values()) {
            const { user } = member;
            if (!registry.users.has(member.id)) {
                Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                await registry.users.addUser(member);
            }
            else {
                const guildMember = guildMembers.find(gm => gm.user_id === member.id);
                if (!guildMember) {
                    await database.guildMembers.add(member);
                    Log.info(`Added new member ${Format.member(member)}.`);
                }
                else {
                    guildMember.nowAlive = true;
                    if (guildMember.first_joined_at === '9223372036854775807') {
                        await database.guildMembers.fixInvalidJoinDate(member);
                    }
                }

                const latestUsername = latestUsernames.find(u => u.user_id === user.id);
                if (!latestUsername || latestUsername.username !== user.username) {
                    await database.usernames.add(user);
                    latestUsernames = latestUsernames.filter(u => u.user_id !== user.id);
                    latestUsernames.push({ 'user_id': user.id, 'username': user.username });
                    Log.info(`Added new username for ${Format.user(user)}.`);
                }
            }
        }

        const aliveChanged = guildMembers.filter(gm => gm.alive !== gm.nowAlive).map(gm => gm.user_id);
        if (aliveChanged.length > 0) {
            await database.guildMembers.reverseAliveInGuild(guild, aliveChanged);
        }
    }
}

module.exports = GuildCreate;