'use strict';

const path = require('path');
const fs = require('fs');

const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));
const intervalsPath = GlobalPaths.pathMapper.intervals.path;

class IntervalRunner {
    constructor() {
        this._intervals = new Map();
        this._loadAll();
    }

    startAll() {
        this._intervals.forEach(interval => this._start(interval));
    }

    _start(interval) {
        if (interval.enabled) {
            interval.runningInterval = setInterval(interval.interval, interval.intervalTime)
        }
    }

    stopAll() {
        this._intervals.forEach(interval => this._stop(interval));
    }

    _stop(interval) {
        if (interval.runningInterval !== null) {
            clearInterval(interval.runningInterval);
            interval.runningInterval = null;
        }
    }

    _loadAll() {
        const files = fs.readdirSync(intervalsPath);
        files.forEach(filename => {
            const filePath = path.parse(filename);
            if (filePath.ext === '.js') {
                this.load(filePath.name);
            }
        });
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
    }

    load(intervalName) {
        if (this._intervals.has(intervalName))
            throw new Error(`Interval ${intervalName} is already loaded.`);
        
        const interval = require(path.join(intervalsPath, intervalName));
        this._intervals.set(intervalName, interval);
    }
}

module.exports = new IntervalRunner();