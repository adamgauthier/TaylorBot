'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const taylorbot = require(GlobalPaths.taylorBotClient);
const database = require(GlobalPaths.databaseDriver);

class Ready extends EventHandler {
    constructor() {
        super(async () => {
            Log.info('Client is ready!');

            taylorbot.intervalRunner.startAll();
            Log.info('Intervals started!');

            Log.info('Checking new guilds, users and usernames...');
            await this.syncDatabase();
            Log.info('New guilds, users and usernames checked!');
        });
    }

    async syncDatabase() {
        const startupTime = new Date().getTime();
        const guildMembers = await database.getAllGuildMembers();
        let latestUsernames = await database.getLatestUsernames();

        for (const guild of taylorbot.guilds.values()) {
            if (!taylorbot.guildSettings.has(guild.id)) {
                Log.warn(`Found new guild ${Format.guild(guild)}.`);
                await taylorbot.guildSettings.addGuild(guild);
            }

            const { members } = await guild.fetchMembers();
            for (const member of members.values()) {
                const { user } = member;
                if (!taylorbot.userSettings.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await taylorbot.userSettings.addUser(user);
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

module.exports = new Ready();