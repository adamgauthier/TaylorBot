'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class UserRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.users.find({}, {
                fields: ['user_id', 'ignore_until']
            });
        }
        catch (e) {
            Log.error(`Getting all users: ${e}`);
            throw e;
        }
    }

    mapUserToDatabase(user) {
        return {
            'user_id': user.id
        };
    }

    async get(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.findOne(databaseUser);
        }
        catch (e) {
            Log.error(`Getting user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(member, discoveredAt) {
        const { user, guild, joinedTimestamp } = member;

        try {
            return await this._db.instance.tx(async t => {
                const inserted = await t.one(
                    'INSERT INTO public.users (user_id) VALUES ($1) RETURNING *;',
                    [user.id]
                );
                await t.none(
                    'INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES ($1, $2, $3);',
                    [guild.id, user.id, joinedTimestamp]
                );
                await t.none(
                    'INSERT INTO public.usernames (user_id, username, changed_at) VALUES ($1, $2, $3);',
                    [user.id, user.username, discoveredAt]
                );

                return inserted;
            });
        }
        catch (e) {
            Log.error(`Adding new user from member ${Format.member(member)}: ${e}`);
            throw e;
        }
    }
}

module.exports = UserRepository;