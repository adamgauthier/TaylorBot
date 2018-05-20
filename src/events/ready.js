'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class Ready extends EventHandler {
    async handler(client) {
        Log.info('Client is ready!');

        client.intervalRunner.startAll();
        Log.info('Intervals started!');

        Log.info('Checking new guilds, users and usernames...');
        await this.syncDatabase(client);
        Log.info('New guilds, users and usernames checked!');
    }

    async syncDatabase(client) {
        const { registry, database } = client.master;

        const startupTime = new Date().getTime();
        const guildMembers = await database.guildMembers.getAll();
        const latestGuildNames = await database.guildNames.getAllLatest();
        let latestUsernames = await database.usernames.getAllLatest();

        for (const guild of client.guilds.values()) {
            if (!registry.guilds.has(guild.id)) {
                Log.warn(`Found new guild ${Format.guild(guild)}.`);
                await registry.guilds.addGuild(guild);
                await database.guildNames.add(guild, startupTime);
            }
            else {
                const latestGuildName = latestGuildNames.find(gn => gn.guild_id === guild.id);
                if (!latestGuildName || guild.name !== latestGuildName.guild_name) {
                    await database.guildNames.add(guild, startupTime);
                    Log.info(`Added new guild name for ${Format.guild(guild)}.${latestGuildName ? ` Old guild name was ${latestGuildName.guild_name}.` : ''}`);
                }
            }

            const members = await guild.members.fetch();
            for (const member of members.values()) {
                const { user } = member;
                if (!registry.users.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await registry.users.addUser(user);
                    await database.guildMembers.add(member);
                    await database.usernames.add(user, startupTime);
                }
                else {
                    if (!guildMembers.some(gm => gm.guild_id === guild.id && gm.user_id === member.id)) {
                        Log.warn(`Found new member ${Format.member(member)}.`);
                        await database.guildMembers.add(member);
                    }
                    else {
                        database.guildMembers.fixInvalidJoinDate(member);
                    }

                    const latestUsername = latestUsernames.find(u => u.user_id === user.id);
                    if (!latestUsername || latestUsername.username !== user.username) {
                        Log.warn(`Found new username for ${Format.user(user)}.`);
                        await database.usernames.add(user, startupTime);
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