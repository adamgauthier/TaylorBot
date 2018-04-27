'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class Ready extends EventHandler {
    async handler(client) {
        Log.info('Client is ready!');

        client.oldRegistry.commands.syncDisabledGuildCommands();
        Log.info('Synced disabled guild commands!');

        client.intervalRunner.startAll();
        Log.info('Intervals started!');

        Log.info('Checking new guilds, users and usernames...');
        await this.syncDatabase(client);
        Log.info('New guilds, users and usernames checked!');

        client.oldRegistry.guilds.syncPrefixes();
        Log.info('Synced guild prefixes!');
    }

    async syncDatabase(client) {
        const { database, oldRegistry } = client;

        const startupTime = new Date().getTime();
        const guildMembers = await database.getAllGuildMembers();
        const latestGuildNames = await database.getLatestGuildNames();
        let latestUsernames = await database.getLatestUsernames();

        for (const guild of client.guilds.values()) {
            if (!oldRegistry.guilds.has(guild.id)) {
                Log.warn(`Found new guild ${Format.guild(guild)}.`);
                await oldRegistry.guilds.addGuild(guild);
                await database.addGuildName(guild, startupTime);
            }
            else {
                const latestGuildName = latestGuildNames.find(gn => gn.guild_id === guild.id);
                if (!latestGuildName || guild.name !== latestGuildName.guild_name) {
                    await database.addGuildName(guild, startupTime);
                    Log.info(`Added new guild name for ${Format.guild(guild)}.${latestGuildName ? ` Old guild name was ${latestGuildName.guild_name}.` : ''}`);
                }
            }

            const members = await guild.members.fetch();
            for (const member of members.values()) {
                const { user } = member;
                if (!oldRegistry.users.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await oldRegistry.users.addUser(user);
                    await database.addGuildMember(member);
                    await database.addUsername(user, startupTime);
                }
                else {
                    if (!guildMembers.some(gm => gm.guild_id === guild.id && gm.user_id === member.id)) {
                        Log.warn(`Found new member ${Format.member(member)}.`);
                        await database.addGuildMember(member);
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
}

module.exports = Ready;