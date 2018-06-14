'use strict';

const fs = require('fs').promises;
const path = require('path');

const { Paths } = require('globalobjects');

const eventsPath = Paths.eventsFolderPath;

const requireEvent = eventName => require(path.join(eventsPath, eventName));

class EventLoader {
    static async loadAll(client) {
        const files = await fs.readdir(eventsPath);

        return files
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(file => {
                const Event = requireEvent(file.base);
                const event = new Event();
                if (event.enabled)
                    client.on(file.name, (...args) => event.handler(client, ...args));
            });
    }
}

module.exports = EventLoader;