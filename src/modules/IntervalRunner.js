'use strict';

const path = require('path');
const fs = require('fs/promises');

const { Paths } = require('globalobjects');
const intervalsPath = Paths.intervalsFolderPath;

const Log = require(Paths.Logger);

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
                Log.warn(`Attempted to start Interval ${interval.name} when it was already running.`);
                this._stop(interval);
            }
            interval.runningInterval = setInterval(() => interval.interval(this._client), interval.intervalTime);
            Log.verbose(`Started Interval ${interval.name}.`);
        }
    }

    stopAll() {
        this._intervals.forEach(interval => this._stop(interval));
    }

    _stop(interval) {
        if (interval.runningInterval) {
            clearInterval(interval.runningInterval);
            interval.runningInterval = null;
            Log.verbose(`Stopped Interval ${interval.name}.`);
        }
    }

    async loadAll() {
        const files = await fs.readdir(intervalsPath);
        files
            .map(path.parse)
            .filter(f => f.ext === '.js')
            .forEach(f => this.load(f.name));
    }

    reload(intervalName) {
        this.unload(intervalName);
        this.load(intervalName);
        this._start(this._intervals.get(intervalName));
    }

    unload(intervalName) {
        if (!this._intervals.has(intervalName))
            throw new Error(`Interval ${intervalName} is not loaded.`);
        
        this._stop(this._intervals.get(intervalName));
        this._intervals.delete(intervalName);
        Log.verbose(`Unloaded Interval ${intervalName}.`);
    }

    load(intervalName) {
        if (this._intervals.has(intervalName))
            throw new Error(`Interval ${intervalName} is already loaded.`);
        
        const Interval = require(path.join(intervalsPath, intervalName));
        const interval = new Interval();
        interval.name = intervalName;
        this._intervals.set(intervalName, interval);
        Log.verbose(`Loaded Interval ${intervalName}.`);
    }
}

module.exports = IntervalRunner;