'use strict';

const fs = require('fs/promises');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const inhibitorsPath = GlobalPaths.inhibitorsFolderPath;

const requireInhibitor = inhibitorName => require(path.join(inhibitorsPath, inhibitorName));

class InhibitorLoader {
    static async loadAll() {
        const files = await fs.readdir(inhibitorsPath);

        return files
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(file => {
                const Inhibitor = requireInhibitor(file.base);
                return new Inhibitor();
            });
    }
}

module.exports = InhibitorLoader;