'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');
const GuildMemberJoinedLoggable = require('../modules/logging/GuildMemberJoinedLoggable.js');
const GuildMemberRejoinedLoggable = require('../modules/logging/GuildMemberRejoinedLoggable.js');

class GuildMemberAdd extends EventHandler {
    constructor() {
        super(Events.GUILD_MEMBER_ADD);
    }

    async handler(client, member) {
        const { user, guild } = member;
        const { registry, database } = client.master;

        let loggable;

        if (!registry.users.has(member.id)) {
            Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
            await registry.users.addUser(member);
            loggable = new GuildMemberJoinedLoggable(member);
        }
        else {
            const databaseMember = await database.guildMembers.get(member);
            if (!databaseMember) {
                await database.guildMembers.add(member);
                Log.info(`Added new member ${Format.member(member)}.`);
                loggable = new GuildMemberJoinedLoggable(member);
            }
            else {
                const updated = await database.guildMembers.fixInvalidJoinDate(member);
                loggable = updated ?
                    new GuildMemberRejoinedLoggable(member) :
                    new GuildMemberRejoinedLoggable(member, databaseMember.first_joined_at);

                await database.guildMembers.setAlive(member);
            }

            const latestUsername = await database.usernames.getLatest(user);
            if (!latestUsername || latestUsername.username !== user.username) {
                await database.usernames.add(user);
                Log.info(`Added new username for ${Format.user(user)}.`);
            }
        }

        client.textChannelLogger.log(guild, loggable);
    }
}

module.exports = GuildMemberAdd;