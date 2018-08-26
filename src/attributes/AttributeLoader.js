'use strict';

const fs = require('fs').promises;
const path = require('path');

const GetMemberAttributeCommand = require('./GetMemberAttributeCommand.js');

const memberAttributesPath = path.join(__dirname, 'member');

const requireMemberAttribute = attributeName => require(path.join(memberAttributesPath, attributeName));

class AttributeLoader {
    static async loadMemberAttributes() {
        const files = await fs.readdir(memberAttributesPath);

        return files
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(file => {
                const Attribute = requireMemberAttribute(file.base);
                return new Attribute();
            });
    }

    static async loadMemberAttributeCommands() {
        const memberAttributes = await AttributeLoader.loadMemberAttributes();

        return memberAttributes.map(a => new GetMemberAttributeCommand(a));
    }
}

module.exports = AttributeLoader;