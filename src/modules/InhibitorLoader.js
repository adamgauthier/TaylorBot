'use strict';

const fs = require('fs');
const path = require('path');

const { GlobalPaths } = require('globalobjects');

const inhibitorsPath = GlobalPaths.inhibitorsFolderPath;

const requireInhibitor = inhibitorName => require(path.join(inhibitorsPath, inhibitorName));

class InhibitorLoader {
    static loadAll() {
        return new Promise((resolve, reject) => {
            fs.readdir(inhibitorsPath, (err, files) => {
                if (err) reject(err);
                else {
                    const inhibitors = [];
                    files.forEach(filename => {
                        const filePath = path.parse(filename);
                        if (filePath.ext === '.js') {
                            const Inhibitor = requireInhibitor(filePath.base);
                            inhibitors.push(new Inhibitor());
                        }
                    });
                    resolve(inhibitors);
                }
            });
        });
    }
}

module.exports = InhibitorLoader;