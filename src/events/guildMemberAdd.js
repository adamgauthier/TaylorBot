'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildMemberAdd extends EventHandler {
    async handler(client, member) {
        const { user } = member;
        const { registry, database } = client.master;

        if (!registry.users.has(member.id)) {
            Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(member.guild)}.`);
            await registry.users.addUser(user);
            await database.guildMembers.add(member);
            await database.usernames.add(user, member.joinedTimestamp);
        }
        else {
            const guildMemberExists = await database.guildMembers.exists(member);
            if (!guildMemberExists) {
                await database.guildMembers.add(member);
                Log.info(`Added new member ${Format.member(member)}.`);
            }
            else {
                database.guildMembers.fixInvalidJoinDate(member);
            }

            const latestUsername = await database.usernames.getLatest(user);
            if (!latestUsername || latestUsername.username !== user.username) {
                await database.usernames.add(user, member.joinedTimestamp);
                Log.info(`Added new username for ${Format.user(user)}.`);
            }
        }
    }
}

module.exports = GuildMemberAdd;