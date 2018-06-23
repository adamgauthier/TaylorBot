'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildCreate extends EventHandler {
    constructor() {
        super(Events.GUILD_CREATE);
    }

    async handler(client, guild) {
        Log.info(`Joined guild ${Format.guild(guild)}.`);

        const { registry, database } = client.master;

        const members = await guild.members.fetch();
        const joinTime = members.get(client.user.id).joinedTimestamp;

        if (!registry.guilds.has(guild.id)) {
            Log.info(`Adding new guild ${Format.guild(guild)}.`);
            await registry.guilds.addGuild(guild, joinTime);
        }
        else {
            const latestGuildName = await database.guildNames.getLatest(guild);
            if (!latestGuildName || guild.name !== latestGuildName.guild_name) {
                await database.guildNames.add(guild, joinTime);
                Log.info(`Added new guild name for ${Format.guild(guild)}.${latestGuildName ? ` Old guild name was ${latestGuildName.guild_name}.` : ''}`);
            }
        }

        const channels = await database.textChannels.getAllInGuild(guild);
        for (const textChannel of guild.channels.filter(c => c.type === 'text').values()) {
            if (!channels.some(c => c.channel_id === textChannel.id)) {
                Log.info(`Found new text channel ${Format.channel(textChannel)}.`);
                await database.textChannels.add(textChannel, joinTime);
            }
        }

        const guildMembers = await database.guildMembers.getAllInGuild(guild);
        let latestUsernames = await database.usernames.getAllLatest();

        for (const member of members.values()) {
            const { user } = member;
            if (!registry.users.has(member.id)) {
                Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                await registry.users.addUser(member, joinTime);
            }
            else {
                if (!guildMembers.some(gm => gm.user_id === member.id)) {
                    await database.guildMembers.add(member);
                    Log.info(`Added new member ${Format.member(member)}.`);
                }

                const latestUsername = latestUsernames.find(u => u.user_id === user.id);
                if (!latestUsername || latestUsername.username !== user.username) {
                    await database.usernames.add(user, joinTime);
                    latestUsernames = latestUsernames.filter(u => u.user_id !== user.id);
                    latestUsernames.push({ 'user_id': user.id, 'username': user.username });
                    Log.info(`Added new username for ${Format.user(user)}.`);
                }
            }
        }
    }
}

module.exports = GuildCreate;