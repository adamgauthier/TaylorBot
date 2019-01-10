'use strict';

const Log = require('../tools/Logger.js');
const IntervalLoader = require('./IntervalLoader.js');

class IntervalRunner {
    constructor(client) {
        this._client = client;
        this._intervals = new Map();
    }

    startAll() {
        this._intervals.forEach(interval => this._start(interval));
    }

    _start(interval) {
        if (interval.enabled) {
            if (interval.runningInterval) {
                Log.warn(`Attempted to start Interval ${interval.id} when it was already running.`);
                this._stop(interval);
            }
            interval.runningInterval = setInterval(() => interval.interval(this._client), interval.intervalMs);
            Log.verbose(`Started Interval ${interval.id}.`);
        }
    }

    stopAll() {
        this._intervals.forEach(interval => this._stop(interval));
    }

    _stop(interval) {
        if (interval.runningInterval) {
            clearInterval(interval.runningInterval);
            interval.runningInterval = null;
            Log.verbose(`Stopped Interval ${interval.id}.`);
        }
    }

    async loadAll() {
        for (const interval of await IntervalLoader.loadAll()) {
            this.load(interval);
        }
    }

    reload(intervalId) {
        this.unload(intervalId);
        this.load(intervalId);
        this._start(this._intervals.get(intervalId));
    }

    unload(intervalId) {
        if (!this._intervals.has(intervalId))
            throw new Error(`Interval ${intervalId} is not loaded.`);

        this._stop(this._intervals.get(intervalId));
        this._intervals.delete(intervalId);
        Log.verbose(`Unloaded Interval ${intervalId}.`);
    }

    load(interval) {
        if (this._intervals.has(interval.id))
            throw new Error(`Interval ${interval.id} is already loaded.`);

        this._intervals.set(interval.id, interval);
        Log.verbose(`Loaded Interval ${interval.id}.`);
    }
}

module.exports = IntervalRunner;