'use strict';

const fs = require('fs').promises;
const path = require('path');
const { Events } = require('discord.js').Constants;

const { Paths } = require('globalobjects');

const eventsPath = Paths.eventsFolderPath;

class EventLoader {
    static async loadAll(client) {
        const files = await fs.readdir(eventsPath);

        return files
            .map(file => path.join(eventsPath, file))
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(path.format)
            .map(file => require(file))
            .map(EventHandler => new EventHandler())
            .filter(event => event.enabled)
            .forEach(event => {
                if (!Object.values(Events).includes(event.eventName))
                    throw new Error(`Event name ${event.eventName} does not exist.`);

                client.on(event.eventName, (...args) => event.handler(client, ...args));
            });
    }
}

module.exports = EventLoader;