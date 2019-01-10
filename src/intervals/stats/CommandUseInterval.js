'use strict';

const Interval = require('../Interval.js');
class CommandUseInterval extends Interval {
    constructor() {
        super({
            id: 'command-use-count-updater',
            intervalMs: 5 * 60 * 1000
        });
    }

    interval({ master }) {
        const { registry, database } = master;

        const cache = Array.from(registry.commands.useCountCache.entries());
        registry.commands.useCountCache.clear();

        return Promise.all(cache.map(
            ([commandName, { count, errorCount }]) => database.commands.addUseCount([commandName], count, errorCount)
        ));
    }
}

module.exports = CommandUseInterval;