'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class Ready extends EventHandler {
    constructor() {
        super();
    }

    async handler(taylorbot) {
        Log.info('Client is ready!');

        taylorbot.intervalRunner.startAll();
        Log.info('Intervals started!');

        Log.info('Checking new guilds, users and usernames...');
        await this.syncDatabase(taylorbot);
        Log.info('New guilds, users and usernames checked!');
    }

    async syncDatabase(taylorbot) {
        const { database, registry } = taylorbot;

        const startupTime = new Date().getTime();
        const guildMembers = await database.getAllGuildMembers();
        let latestUsernames = await database.getLatestUsernames();

        for (const guild of taylorbot.guilds.values()) {
            if (!registry.guilds.has(guild.id)) {
                Log.warn(`Found new guild ${Format.guild(guild)}.`);
                await registry.guilds.addGuild(guild);
            }

            const { members } = await guild.fetchMembers();
            for (const member of members.values()) {
                const { user } = member;
                if (!registry.users.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await registry.users.addUser(user);
                }

                if (!guildMembers.some(gm => gm.guild_id === guild.id && gm.user_id === member.id)) {
                    Log.warn(`Found new member ${Format.member(member)}.`);
                    await database.addGuildMember(member);
                    Log.verbose(`Added new member ${Format.member(member)}.`);
                }

                const latestUsername = latestUsernames.find(u => u.user_id === user.id);
                if (!latestUsername || latestUsername.username !== user.username) {
                    Log.warn(`Found new username for ${Format.user(user)}.`);
                    await database.addUsername(user, startupTime);
                    latestUsernames = latestUsernames.filter(u => u.user_id !== user.id);
                    latestUsernames.push({ 'user_id': user.id, 'username': user.username });
                    Log.verbose(`Added new username for ${Format.user(user)}.`);
                }
            }
        }
    }
}

module.exports = Ready;