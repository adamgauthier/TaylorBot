'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const intervalRunner = require(GlobalPaths.intervalRunner);
const taylorbot = require(GlobalPaths.taylorBotClient);
const database = require(GlobalPaths.databaseDriver);

class Ready extends EventHandler {
    constructor() {
        super(async () => {
            console.log('Client is ready!');
            
            intervalRunner.startAll();
            console.log('Intervals started!');

            console.log('Checking new users...');
            await this._checkNewUsers();
            console.log('New users checked!');

            console.log('Checking new members...');
            await this._checkNewMembers();
            console.log('New members checked!');
        });
    }

    _checkNewMembers() {
        return Promise.all(taylorbot.guilds.map(guild => {            
            return Promise.all(guild.members.map(async m => {
                const {user: u, guild: g} = m;
                const guildUserExists = await database.guildUserExists(m);
                if (guildUserExists === false) {
                    console.log(`${u.username} (${u.id}) for guild ${g.name} (${g.id}) did not exist, adding.`);
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
                console.log(`${user.username} (${user.id}) did not exists, adding.`);
                await database.addNewUser(user);
            }
            return await database.addNewUsername(user, initTime, true);
        }));
    }
}

module.exports = new Ready();