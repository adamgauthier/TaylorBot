'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildCreate extends EventHandler {
    async handler(taylorbot, guild) {
        Log.info(`Joined guild ${Format.guild(guild)}.`);

        const { database, oldRegistry } = taylorbot;

        const clientMember = await guild.fetchMember(taylorbot.user);
        const joinTime = clientMember.joinedTimestamp;
        const guildMembers = await database.getAllGuildMembersInGuild(guild);
        let latestUsernames = await database.getLatestUsernames();

        if (!oldRegistry.guilds.has(guild.id)) {
            Log.info(`Adding new guild ${Format.guild(guild)}.`);
            await oldRegistry.guilds.addGuild(guild);
        }

        const { members } = await guild.fetchMembers();
        for (const member of members.values()) {
            const { user } = member;
            if (!oldRegistry.users.has(member.id)) {
                Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                await oldRegistry.users.addUser(user);
            }

            if (!guildMembers.some(gm => gm.user_id === member.id)) {
                await database.addGuildMember(member);
                Log.info(`Added new member ${Format.member(member)}.`);
            }

            const latestUsername = latestUsernames.find(u => u.user_id === user.id);
            if (!latestUsername || latestUsername.username !== user.username) {
                await database.addUsername(user, joinTime);
                latestUsernames = latestUsernames.filter(u => u.user_id !== user.id);
                latestUsernames.push({ 'user_id': user.id, 'username': user.username });
                Log.info(`Added new username for ${Format.user(user)}.`);
            }
        }
    }
}

module.exports = GuildCreate;