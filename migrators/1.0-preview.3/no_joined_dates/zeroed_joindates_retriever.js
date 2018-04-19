const sqlite3 = require('sqlite3').verbose();
const fs = require('fs');

const main = () => {
    const db = new sqlite3.Database('old_database.db');

    db.all('SELECT userByServer.`id` AS id, usernames.`username` AS username FROM userByServer INNER JOIN usernames ON userByServer.`id` = usernames.`userId` WHERE `serverId` = ? AND userByServer.firstJoinedAt = 0 GROUP BY usernames.`userId`;', ['115332333745340416'],
        async (err, rows) => {
            if (err) {
                throw err;
            }
            else {
                fs.writeFileSync('./no_join_dates.json', JSON.stringify(rows, null, 2));
            }
        });
};

main();