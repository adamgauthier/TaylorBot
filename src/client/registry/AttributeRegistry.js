'use strict';

const AttributeLoader = require('../../attributes/AttributeLoader.js');
const Log = require('../../tools/Logger.js');

class AttributeRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async loadAll() {
        const databaseAttributes = await this.database.attributes.getAll();

        const attributes = [
            ...(await AttributeLoader.loadMemberAttributes()),
            ...(await AttributeLoader.loadUserAttributes())
        ];

        const databaseAttributesNotInFiles = databaseAttributes.filter(
            databaseAttribute => !attributes.some(a => a.id === databaseAttribute.attribute_id)
        );

        if (databaseAttributesNotInFiles.length > 0)
            throw new Error(`Found database attributes not in files: ${databaseAttributesNotInFiles.map(da => da.id).join(',')}.`);

        const fileAttributesNotInDatabase = attributes.filter(
            attribute => !databaseAttributes.some(da => da.attribute_id === attribute.id)
        );

        if (fileAttributesNotInDatabase.length > 0) {
            Log.warn(`Found file attributes not in database: ${fileAttributesNotInDatabase.map(a => a.id).join(',')}.`);

            const inserted = await this.database.attributes.addAll(
                fileAttributesNotInDatabase.map(attribute => {
                    return { 'attribute_id': attribute.id, 'created_at': Date.now() };
                })
            );

            databaseAttributes.push(...inserted);
        }

        attributes.forEach(a => this.set(a.id, a));
    }
}

module.exports = AttributeRegistry;