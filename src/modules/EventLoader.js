'use strict';

const fs = require('fs');
const path = require('path');

const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const eventsPath = GlobalPaths.pathMapper.events.path;

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
                            const event = requireEvent(filePath.base);
                            if (event.enabled)
                                client.on(filePath.name, event.handler);
                        }
                    });
                    resolve();
                }
            });
        });
    }
}

module.exports = EventLoader;