'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const intervalRunner = require(GlobalPaths.intervalRunner);
const taylorbot = require(GlobalPaths.taylorBotClient);
const database = require(GlobalPaths.databaseDriver);

class Ready extends EventHandler {
    constructor() {
        super(async () => {
            Log.info('Client is ready!');

            intervalRunner.startAll();
            Log.info('Intervals started!');

            Log.info('Checking new guilds...');
            await this.checkNewGuilds();
            Log.info('New guilds checked!');

            Log.info('Checking new users...');
            await this.checkNewUsers();
            Log.info('New users checked!');

            // Log.info('Checking new users...');
            // await this._checkNewUsers();
            // Log.info('New users checked!');
        });
    }

    checkNewGuilds() {
        return Promise.all(
            taylorbot.guilds.map(async guild => {
                if (!taylorbot.guildSettings.has(guild.id)) {
                    Log.warn(`Found new guild ${Format.formatGuild(guild)}.`);
                    await taylorbot.guildSettings.addGuild(guild);
                }
            })
        );
    }

    async checkNewUsers() {
        const guildMembers = await database.getAllGuildMembers();

        for (const guild of taylorbot.guilds.values()) {
            const { members } = await guild.fetchMembers();
            for (const member of members.values()) {
                if (!taylorbot.userSettings.has(member.id)) {
                    const { user } = member;
                    Log.warn(`Found new user ${Format.formatUser(user)} in guild ${Format.formatGuild(guild)}.`);
                    await taylorbot.userSettings.addUser(user);
                }

                if (!guildMembers.some(gm => gm.guild_id === guild.id && gm.user_id === member.id)) {
                    Log.warn(`Found new member ${Format.formatMember(member)}.`);
                    await database.addGuildMember(member);
                    Log.verbose(`Added new member ${Format.formatMember(member)}.`);
                }
            }
        }
    }

    _checkNewUsers() {
        const initTime = new Date().getTime();
        return Promise.all(taylorbot.users.map(async user => {
            const userExists = await database.userExists(user);
            if (userExists === false) {
                Log.info(`${Format.formatUser(u)} did not exists, adding.`);
                await database.addNewUser(user);
            }
            return await database.addNewUsername(user, initTime, true);
        }));
    }
}

module.exports = new Ready();