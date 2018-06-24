const moment = require('moment');
const fs = require('fs');
const users = require('./luna_users.json');
const noJoinDates = require('./no_join_dates.json');
const massive = require('massive');
const PostgreSQLConfig = require('../../postgresql.json');

const notUpdated = [];
const updateAnnouncement = [];
const updateFirstMessage = [];

const prepare = () => {
    for (const id in users) {
        const user = users[id];

        if (user.fearlessbot_announcement_timestamp !== null) {
            const announcementTimestamp = `${user.fearlessbot_announcement_timestamp} -04:00`;
            const m = moment.utc(announcementTimestamp, 'YYYY-MM-DD HH:mm:ss ZZ', true);

            if (!m.isValid())
                throw new Error(`announcement ${id} : ${user.fearlessbot_announcement_timestamp}`);

            updateAnnouncement.push({
                id,
                'joinedTimestamp': m.format('x')
            });
        }
        else if (user.first_message_timestamp !== null) {
            const firstMessageTimestamp = `${user.first_message_timestamp} -04:00`;
            const m = moment.utc(firstMessageTimestamp, 'YYYY-MM-DD HH:mm:ss ZZ', true);

            if (!m.isValid())
                throw new Error(`first message ${id} : ${user.first_message_timestamp}`);

            updateFirstMessage.push({
                id,
                'joinedTimestamp': m.format('x')
            });
        }
        else {
            notUpdated.push(id);
        }
    }
};

const update = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    for (const users of [updateAnnouncement, updateFirstMessage]) {
        for (const user of users) {
            const criteria = {
                'user_id': user.id,
                'guild_id': '115332333745340416'
            };

            const guildMember = await pg_db.guild_members.find(criteria, {
                'single': true
            });

            user.original_first_joined_at = guildMember.first_joined_at;
            user.last_recorded_username = noJoinDates.find(u => u.id === user.id).username;

            await pg_db.guild_members.update(
                criteria,
                {
                    'first_joined_at': user.joinedTimestamp
                }
            );
        }
    }
};

prepare();
update().then(() => {
    const updatedDir = './updated';
    if (!fs.existsSync(updatedDir)){
        fs.mkdirSync(updatedDir);
    }
    fs.writeFileSync(`${updatedDir}/not-updated.json`, JSON.stringify(notUpdated, null, 2));
    fs.writeFileSync(`${updatedDir}/updated_announcement.json`, JSON.stringify(updateAnnouncement, null, 2));
    fs.writeFileSync(`${updatedDir}/updated_first_message.json`, JSON.stringify(updateFirstMessage, null, 2));
});