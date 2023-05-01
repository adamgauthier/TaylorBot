import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { DeprecatedMemberPresenter } from '../member-presenters/DeprecatedMemberPresenter';
import { GuildMember, Guild } from 'discord.js';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

class TaypointsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'taypoints',
            aliases: ['points'],
            description: 'taypoints',
            columnName: 'taypoint_count',
            singularName: 'taypoint',
            presenter: DeprecatedMemberPresenter
        });
    }

    async getCommand(commandContext: CommandMessageContext, member: GuildMember): Promise<null> {
        return null;
    }

    async retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        throw new Error('Deprecated');
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return Promise.resolve(['**/taypoints leaderboard**']);
    }
}

export = TaypointsMemberAttribute;
