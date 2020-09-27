import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient';
import { Interval } from './Interval';
import { IntervalLoader } from './IntervalLoader';

export class IntervalRunner {
    #client: TaylorBotClient;
    #intervals: Map<string, Interval>;

    constructor(client: TaylorBotClient) {
        this.#client = client;
        this.#intervals = new Map();
    }

    startAll(): void {
        this.#intervals.forEach(interval => this._start(interval));
    }

    _start(interval: Interval): void {
        if (interval.enabled) {
            if (interval.runningInterval) {
                Log.warn(`Attempted to start Interval ${interval.id} when it was already running.`);
                this._stop(interval);
            }
            interval.runningInterval = setInterval(() => interval.interval(this.#client), interval.intervalMs);
            Log.verbose(`Started Interval ${interval.id}.`);
        }
    }

    stopAll(): void {
        this.#intervals.forEach(interval => this._stop(interval));
    }

    _stop(interval: Interval): void {
        if (interval.runningInterval) {
            clearInterval(interval.runningInterval);
            interval.runningInterval = null;
            Log.verbose(`Stopped Interval ${interval.id}.`);
        }
    }

    async loadAll(): Promise<void> {
        for (const interval of await IntervalLoader.loadAll()) {
            this.load(interval);
        }
    }

    load(interval: Interval): void {
        if (this.#intervals.has(interval.id))
            throw new Error(`Interval ${interval.id} is already loaded.`);

        this.#intervals.set(interval.id, interval);
        Log.verbose(`Loaded Interval ${interval.id}.`);
    }
}
