'use strict';

const InhibitorLoader = require('../../inhibitors/InhibitorLoader.js');
const SilentInhibitor = require('../../inhibitors/SilentInhibitor.js');
const NoisyInhibitor = require('../../inhibitors/NoisyInhibitor.js');

class InhibitorRegistry {
    constructor() {
        this.silentInhibitors = new Set();
        this.noisyInhibitors = new Set();
    }

    async loadAll() {
        const inhibitors = await InhibitorLoader.loadAll();

        inhibitors.forEach(inhibitor => {
            if (inhibitor instanceof SilentInhibitor) {
                this.silentInhibitors.add(inhibitor);
            }
            else if (inhibitor instanceof NoisyInhibitor) {
                this.noisyInhibitors.add(inhibitor);
            }
            else {
                throw new Error(`Invalid inhibitor type for inhibitor ${inhibitor}.`);
            }
        });
    }

    getSilentInhibitors() {
        return this.silentInhibitors.values();
    }

    getNoisyInhibitors() {
        return this.noisyInhibitors.values();
    }
}

module.exports = InhibitorRegistry;