import { AttributeLoader } from '../../attributes/AttributeLoader.js';
import { Log } from '../../tools/Logger';
import { DatabaseDriver } from '../../database/DatabaseDriver.js';
import { MemberAttribute } from '../../attributes/MemberAttribute.js';
import { UserAttribute } from '../../attributes/UserAttribute.js';

export class AttributeRegistry extends Map<string, string | MemberAttribute | UserAttribute> {
    #database: DatabaseDriver;
    constructor(database: DatabaseDriver) {
        super();
        this.#database = database;
    }

    async loadAll(): Promise<void> {
        const databaseAttributes = await this.#database.attributes.getAll();

        const attributes = [
            ...(await AttributeLoader.loadMemberAttributes()),
            ...(await AttributeLoader.loadUserAttributes())
        ];

        const fileAttributesNotInDatabase = attributes.map(a => ({ id: a.id }))
            .filter(
                attribute => !databaseAttributes.some(da => da.attribute_id === attribute.id)
            );

        if (fileAttributesNotInDatabase.length > 0) {
            Log.warn(`Found file attributes not in database: ${fileAttributesNotInDatabase.map(a => a.id).join(',')}.`);

            await this.#database.attributes.addAll(
                fileAttributesNotInDatabase.map(attribute => {
                    return { 'attribute_id': attribute.id };
                })
            );
        }

        attributes.forEach(c => this.cacheAttribute(c));
    }

    cacheAttribute(attribute: MemberAttribute | UserAttribute): void {
        const key = attribute.id.toLowerCase();

        if (this.has(key))
            throw new Error(`Attribute '${attribute.id}' is already cached.`);

        this.set(key, attribute);

        for (const alias of attribute.aliases) {
            const aliasKey = alias.toLowerCase();

            if (this.has(aliasKey))
                throw new Error(`Attribute Key '${aliasKey}' is already cached when setting alias.`);

            this.set(aliasKey, key);
        }
    }

    getAttribute(id: string): MemberAttribute | UserAttribute {
        const attribute = this.get(id);

        if (!attribute)
            throw new Error(`Attribute '${id}' isn't cached.`);

        if (typeof (attribute) === 'string')
            throw new Error(`Attribute '${id}' is cached as an alias.`);

        return attribute;
    }

    resolve(attributeName: string): MemberAttribute | UserAttribute | undefined {
        const attribute = this.get(attributeName.toLowerCase());

        if (typeof (attribute) === 'string') {
            return this.getAttribute(attribute);
        }

        return attribute;
    }
}
