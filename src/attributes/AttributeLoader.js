'use strict';

const fs = require('fs').promises;
const path = require('path');

const GetMemberAttributeCommand = require('./GetMemberAttributeCommand.js');
const RankMemberAttributeCommand = require('./RankMemberAttributeCommand.js');
const GetUserAttributeCommand = require('./GetUserAttributeCommand.js');
const SetUserAttributeCommand = require('./SetUserAttributeCommand.js');

class AttributeLoader {
    static async loadAttributesIn(dirPath) {
        const files = await fs.readdir(dirPath);

        return files
            .map(file => path.join(dirPath, file))
            .map(file => {
                const Attribute = require(file);
                return new Attribute();
            });
    }

    static loadMemberAttributes() {
        return AttributeLoader.loadAttributesIn(path.join(__dirname, 'member'));
    }

    static loadUserAttributes() {
        return AttributeLoader.loadAttributesIn(path.join(__dirname, 'user'));
    }

    static async loadMemberAttributeCommands() {
        const memberAttributes = await AttributeLoader.loadMemberAttributes();

        return [
            ...memberAttributes.map(a => new GetMemberAttributeCommand(a)),
            ...memberAttributes.map(a => new RankMemberAttributeCommand(a))
        ];
    }

    static async loadUserAttributeCommands() {
        const memberAttributes = await AttributeLoader.loadUserAttributes();

        return [
            ...memberAttributes.map(a => new GetUserAttributeCommand(a)),
            ...memberAttributes.map(a => new SetUserAttributeCommand(a))
        ];
    }
}

module.exports = AttributeLoader;