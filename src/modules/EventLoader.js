'use strict';

const fs = require('fs');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const eventsPath = GlobalPaths.eventsFolderPath;

const requireEvent = eventName => require(path.join(eventsPath, eventName));

class EventLoader {
    loadAll(client) {
        return new Promise((resolve, reject) => {
            fs.readdir(eventsPath, (err, files) => {
                if (err) reject(err);
                else {
                    files.forEach(filename => {
                        const filePath = path.parse(filename);
                        if (filePath.ext === '.js') {
                            const Event = requireEvent(filePath.base);
                            const event = new Event();
                            if (event.enabled)
                                client.on(filePath.name, (...args) => event.handler(client, ...args));
                        }
                    });
                    resolve();
                }
            });
        });
    }
}

module.exports = EventLoader;