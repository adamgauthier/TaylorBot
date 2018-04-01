'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildMemberAdd extends EventHandler {
    constructor() {
        super();
    }

    async handler(taylorbot, member) {
        const { user } = member;
        const { database, oldRegistry } = taylorbot;

        if (!oldRegistry.users.has(member.id)) {
            Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(member.guild)}.`);
            await oldRegistry.users.addUser(user);
        }

        const guildMemberExists = await database.doesGuildMemberExist(member);
        if (!guildMemberExists) {
            await database.addGuildMember(member);
            Log.info(`Added new member ${Format.member(member)}.`);
        }

        const latestUsername = await database.getLatestUsername(user);
        if (!latestUsername || latestUsername.username !== user.username) {
            await database.addUsername(user, member.joinedTimestamp);
            Log.info(`Added new username for ${Format.user(user)}.`);
        }
    }
}

module.exports = GuildMemberAdd;