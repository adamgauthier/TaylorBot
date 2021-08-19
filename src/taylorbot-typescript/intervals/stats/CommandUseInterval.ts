import { Interval } from '../Interval';
import { TaylorBotClient } from '../../client/TaylorBotClient';

class CommandUseInterval extends Interval {
    constructor() {
        super({
            id: 'command-use-count-updater',
            intervalMs: 5 * 60 * 1000
        });
    }

    async interval({ master }: TaylorBotClient): Promise<void> {
        const { registry, database } = master;

        const cache = Array.from(registry.commands.useCountCache.entries());
        registry.commands.useCountCache.clear();

        for (const [commandName, { count, errorCount }] of cache) {
            await database.commands.addUseCount([commandName], count, errorCount);
        }
    }
}

export = CommandUseInterval;
