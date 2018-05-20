'use strict';

const fs = require('fs/promises');
const path = require('path');

const { Paths } = require('globalobjects');

const eventsPath = Paths.eventsFolderPath;

const requireEvent = eventName => require(path.join(eventsPath, eventName));

class EventLoader {
    async loadAll(client) {
        const files = await fs.readdir(eventsPath);

        files.forEach(filename => {
            const filePath = path.parse(filename);
            if (filePath.ext === '.js') {
                const Event = requireEvent(filePath.base);
                const event = new Event();
                if (event.enabled)
                    client.on(filePath.name, (...args) => event.handler(client, ...args));
            }
        });
    }
}

module.exports = EventLoader;