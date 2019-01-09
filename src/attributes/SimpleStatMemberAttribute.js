'use strict';

const MemberAttribute = require('./MemberAttribute.js');
const SimpleStatPresentor = require('./member-presentors/SimpleStatPresentor.js');

class SimpleStatMemberAttribute extends MemberAttribute {
    constructor(options) {
        options.presentor = SimpleStatPresentor;
        super(options);
        this.singularName = options.singularName;
    }

    async retrieve(database, member) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.retrieve.name}() method.`);
    }

    async rank(database, guild, entries) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.rank.name}() method.`);
    }
}

module.exports = SimpleStatMemberAttribute;