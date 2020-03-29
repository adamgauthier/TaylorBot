import { InhibitorLoader } from '../../inhibitors/InhibitorLoader';
import { NoisyInhibitor } from '../../inhibitors/NoisyInhibitor';
import { SilentInhibitor } from '../../inhibitors/SilentInhibitor';

export class InhibitorRegistry {
    #silentInhibitors = new Set<SilentInhibitor>();
    #noisyInhibitors = new Set<NoisyInhibitor>();

    async loadAll(): Promise<void> {
        const inhibitors = await InhibitorLoader.loadAll();

        inhibitors.forEach(inhibitor => {
            if (inhibitor instanceof SilentInhibitor) {
                this.#silentInhibitors.add(inhibitor);
            }
            else if (inhibitor instanceof NoisyInhibitor) {
                this.#noisyInhibitors.add(inhibitor);
            }
            else {
                throw new Error(`Invalid inhibitor type for inhibitor ${inhibitor}.`);
            }
        });
    }

    getSilentInhibitors(): IterableIterator<SilentInhibitor> {
        return this.#silentInhibitors.values();
    }

    getNoisyInhibitors(): IterableIterator<NoisyInhibitor> {
        return this.#noisyInhibitors.values();
    }
}
