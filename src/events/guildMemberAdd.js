'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const taylorbot = require(GlobalPaths.taylorBotClient);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const database = require(GlobalPaths.databaseDriver);

class GuildMemberAdd extends EventHandler {
    constructor() {
        super(async member => {
            const { user } = member;

            if (!taylorbot.userSettings.has(member.id)) {
                Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(member.guild)}.`);
                await taylorbot.userSettings.addUser(user);
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