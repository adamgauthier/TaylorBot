'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildCreate extends EventHandler {
    constructor() {
        super(async (taylorbot, guild) => {
            Log.info(`Joined guild ${Format.guild(guild)}.`);

            const { database, registry } = taylorbot;

            const clientMember = await guild.fetchMember(taylorbot.user);
            const joinTime = clientMember.joinedTimestamp;
            const guildMembers = await database.getAllGuildMembersInGuild(guild);
            let latestUsernames = await database.getLatestUsernames();

            if (!registry.guilds.has(guild.id)) {
                Log.info(`Adding new guild ${Format.guild(guild)}.`);
                await registry.guilds.addGuild(guild);
            }

            const { members } = await guild.fetchMembers();
            for (const member of members.values()) {
                const { user } = member;
                if (!registry.users.has(member.id)) {
                    Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await registry.users.addUser(user);
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
        });
    }
}

module.exports = new GuildCreate();