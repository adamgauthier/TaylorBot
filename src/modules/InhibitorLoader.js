'use strict';

const fs = require('fs/promises');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const inhibitorsPath = GlobalPaths.inhibitorsFolderPath;

const requireInhibitor = inhibitorName => require(path.join(inhibitorsPath, inhibitorName));

class InhibitorLoader {
    static async loadAll() {
        const files = await fs.readdir(inhibitorsPath);

        const inhibitors = [];

        files.forEach(filename => {
            const filePath = path.parse(filename);
            if (filePath.ext === '.js') {
                const Inhibitor = requireInhibitor(filePath.base);
                inhibitors.push(new Inhibitor());
            }
        });

        return inhibitors;
    }
}

module.exports = InhibitorLoader;