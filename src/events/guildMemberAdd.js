'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildMemberAdd extends EventHandler {
    constructor() {
        super(async (taylorbot, member) => {
            const { user } = member;
            const { database, registry } = taylorbot;

            if (!registry.users.has(member.id)) {
                Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(member.guild)}.`);
                await registry.users.addUser(user);
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
        });
    }
}

module.exports = new GuildMemberAdd();