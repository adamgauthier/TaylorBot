import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');

import { GetUserAttributeCommand } from './commands/GetUserAttributeCommand';
import { SetUserAttributeCommand } from './commands/SetUserAttributeCommand';
import { ClearUserAttributeCommand } from './commands/ClearUserAttributeCommand';
import { UserAttribute } from './UserAttribute';
import { SettableUserAttribute } from './SettableUserAttribute';
import { ListAttributeCommand } from './commands/ListAttributeCommand';

export class AttributeLoader {
    private static async loadAttributesIn(dirPath: string): Promise<UserAttribute[]> {
        const files = await fs.readdir(dirPath);

        return files
            .map(file => path.join(dirPath, file))
            .map(require)
            .map((Attribute: new () => UserAttribute) => {
                return new Attribute();
            });
    }

    static async loadUserAttributes(): Promise<UserAttribute[]> {
        const loaded = await AttributeLoader.loadAttributesIn(path.join(__dirname, 'user'));

        if (loaded.every(a => a instanceof UserAttribute))
            return loaded as UserAttribute[];
        else
            throw new Error('Not all member attributes are instances of UserAttribute');
    }

    private static isSettable(attribute: UserAttribute): attribute is SettableUserAttribute {
        return attribute.canSet;
    }

    static async loadUserAttributeCommands(): Promise<(GetUserAttributeCommand | SetUserAttributeCommand | ClearUserAttributeCommand | ListAttributeCommand)[]> {
        const userAttributes = await AttributeLoader.loadUserAttributes();

        return [
            ...userAttributes.map(a => new GetUserAttributeCommand(a)),
            ...userAttributes.filter(AttributeLoader.isSettable).map(a => new SetUserAttributeCommand(a)),
            ...userAttributes.filter(AttributeLoader.isSettable).map(a => new ClearUserAttributeCommand(a)),
            ...userAttributes.filter(a => a.list !== null).map(a => new ListAttributeCommand(a))
        ];
    }
}
