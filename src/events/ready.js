'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const intervalRunner = require(GlobalPaths.intervalRunner);
const taylorbot = require(GlobalPaths.taylorBotClient);
const database = require(GlobalPaths.databaseDriver);

class Ready extends EventHandler {
    constructor() {
        super(async () => {
            Log.info('Client is ready!');

            intervalRunner.startAll();
            Log.info('Intervals started!');

            Log.info('Checking new users...');
            await this._checkNewUsers();
            Log.info('New users checked!');

            Log.info('Checking new members...');
            await this._checkNewMembers();
            Log.info('New members checked!');
        });
    }

    _checkNewMembers() {
        return Promise.all(taylorbot.guilds.map(guild => {
            return Promise.all(guild.members.map(async m => {
                const { user: u, guild: g } = m;
                const guildUserExists = await database.guildUserExists(m);
                if (guildUserExists === false) {
                    Log.info(`${u.username} (${u.id}) for guild ${g.name} (${g.id}) did not exist, adding.`);
                    return await database.addNewMember(m);
                }
            }));
        }));
    }

    _checkNewUsers() {
        const initTime = new Date().getTime();
        return Promise.all(taylorbot.users.map(async user => {
            const userExists = await database.userExists(user);
            if (userExists === false) {
                Log.info(`${user.username} (${user.id}) did not exists, adding.`);
                await database.addNewUser(user);
            }
            return await database.addNewUsername(user, initTime, true);
        }));
    }
}

module.exports = new Ready();