import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');

import { GetMemberAttributeCommand } from './commands/GetMemberAttributeCommand';
import { RankMemberAttributeCommand } from './commands/RankMemberAttributeCommand';
import { GetUserAttributeCommand } from './commands/GetUserAttributeCommand';
import { SetUserAttributeCommand } from './commands/SetUserAttributeCommand';
import { ClearUserAttributeCommand } from './commands/ClearUserAttributeCommand';
import { MemberAttribute } from './MemberAttribute';
import { UserAttribute } from './UserAttribute';
import { SettableUserAttribute } from './SettableUserAttribute';
import { ListAttributeCommand } from './commands/ListAttributeCommand';

export class AttributeLoader {
    private static async loadAttributesIn(dirPath: string): Promise<(MemberAttribute | UserAttribute)[]> {
        const files = await fs.readdir(dirPath);

        return files
            .map(file => path.join(dirPath, file))
            .map(require)
            .map((Attribute: new () => MemberAttribute | UserAttribute) => {
                return new Attribute();
            });
    }

    static async loadMemberAttributes(): Promise<MemberAttribute[]> {
        const loaded = await AttributeLoader.loadAttributesIn(path.join(__dirname, 'member'));

        if (loaded.every(a => a instanceof MemberAttribute))
            return loaded as MemberAttribute[];
        else
            throw new Error('Not all member attributes are instances of MemberAttribute');
    }

    static async loadUserAttributes(): Promise<UserAttribute[]> {
        const loaded = await AttributeLoader.loadAttributesIn(path.join(__dirname, 'user'));

        if (loaded.every(a => a instanceof UserAttribute))
            return loaded as UserAttribute[];
        else
            throw new Error('Not all member attributes are instances of UserAttribute');
    }

    static async loadMemberAttributeCommands(): Promise<(GetMemberAttributeCommand | RankMemberAttributeCommand | ListAttributeCommand)[]> {
        const memberAttributes = await AttributeLoader.loadMemberAttributes();

        return [
            ...memberAttributes.map(a => new GetMemberAttributeCommand(a)),
            ...memberAttributes.map(a => new RankMemberAttributeCommand(a)),
            ...memberAttributes.filter(a => a.list !== null).map(a => new ListAttributeCommand(a))
        ];
    }

    private static isSettable(attribute: UserAttribute): attribute is SettableUserAttribute {
        return attribute.canSet;
    }

    static async loadUserAttributeCommands(): Promise<(GetUserAttributeCommand | SetUserAttributeCommand | ClearUserAttributeCommand | ListAttributeCommand)[]> {
        const memberAttributes = await AttributeLoader.loadUserAttributes();

        return [
            ...memberAttributes.map(a => new GetUserAttributeCommand(a)),
            ...memberAttributes.filter(AttributeLoader.isSettable).map(a => new SetUserAttributeCommand(a)),
            ...memberAttributes.filter(AttributeLoader.isSettable).map(a => new ClearUserAttributeCommand(a)),
            ...memberAttributes.filter(a => a.list !== null).map(a => new ListAttributeCommand(a))
        ];
    }
}
