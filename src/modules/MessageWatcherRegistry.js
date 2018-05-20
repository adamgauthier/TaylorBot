'use strict';

const fs = require('fs/promises');
const path = require('path');

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const watchersPath = Paths.watchersFolderPath;

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
                Log.error(`Message Watcher ${name} Error: ${e}`);
            }            
        });
    }
}

module.exports = MessageWatcherRegistry;