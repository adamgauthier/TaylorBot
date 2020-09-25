import { ArgumentType } from '../../types/ArgumentType';
import { TypeLoader } from '../../types/TypeLoader';

export class TypeRegistry extends Map<string, ArgumentType> {
    async loadAll(): Promise<void> {
        const types = await TypeLoader.loadAll();

        types.forEach(t => this.cacheType(t));
    }

    cacheType(type: ArgumentType): void {
        if (this.has(type.id)) {
            throw new Error(`Can't cache '${type.id}' because this id is already cached.`);
        }

        this.set(type.id, type);
    }

    getType(typeId: string): ArgumentType {
        const type = this.get(typeId);

        if (type === undefined) {
            throw new Error(`Type '${typeId}' is not cached.`);
        }

        return type;
    }
}
