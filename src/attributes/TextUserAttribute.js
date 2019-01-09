'use strict';

const SettableUserAttribute = require('./SettableUserAttribute.js');
const SimplePresentor = require('./user-presentors/SimplePresentor.js');

class TextUserAttribute extends SettableUserAttribute {
    constructor(options) {
        if (options.presentor === undefined)
            options.presentor = SimplePresentor;
        super(options);
        if (new.target === TextUserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    retrieve(database, user) {
        return database.textAttributes.get(this.id, user);
    }

    set(database, user, value) {
        return database.textAttributes.set(this.id, user, value.toString());
    }

    clear(database, user) {
        return database.textAttributes.clear(this.id, user);
    }

    list(database, guild, entries) {
        return database.textAttributes.listInGuild(this.id, guild, entries);
    }

    formatValue(attribute) {
        return attribute.attribute_value;
    }
}

module.exports = TextUserAttribute;