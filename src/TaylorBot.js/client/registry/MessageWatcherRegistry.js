'use strict';

const fs = require('fs').promises;
const path = require('path');

const Log = require('../../tools/Logger.js');

const watchersPath = path.join(__dirname, '..', '..', 'watchers');

const requireWatcher = watcherName => require(path.join(watchersPath, watcherName));

class MessageWatcherRegistry {
    constructor() {
        this._watchers = new Map();
    }

    async loadAll() {
        const files = await fs.readdir(watchersPath);
        files.forEach(filename => {
            const filePath = path.parse(filename);
            if (filePath.ext === '.js') {
                const Watcher = requireWatcher(filePath.base);
                const watcher = new Watcher();
                if (watcher.enabled)
                    this._watchers.set(filePath.name, watcher);
            }
        });
    }

    feedAll(client, message) {
        this._watchers.forEach(async (watcher, name) => {
            try {
                await watcher.messageHandler(client, message);
            }
            catch (e) {
                Log.error(`Message Watcher ${name} Error: \n${e.stack}`);
            }
        });
    }

    getWatcher(name) {
        return this._watchers.get(name);
    }
}

module.exports = MessageWatcherRegistry;