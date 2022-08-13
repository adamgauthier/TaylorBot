import { DiscordUtil } from '../../discord/DiscordUtil';
import { Guild, GuildMember, EmbedBuilder } from 'discord.js';
import { DatabaseDriver } from '../../../database/DatabaseDriver';
import { EmbedPageEditor } from './EmbedPageEditor';

type MemberObject = Record<string, any> & { user_id: string };

export class MemberEmbedDescriptionPageMessage extends EmbedPageEditor<MemberObject[]> {
    readonly #database: DatabaseDriver;
    readonly #guild: Guild;
    readonly #formatLine: (member: GuildMember, attribute: MemberObject) => string;

    constructor(embed: EmbedBuilder, database: DatabaseDriver, guild: Guild, formatLine: (member: GuildMember, attribute: MemberObject) => string) {
        super(embed);
        this.#database = database;
        this.#guild = guild;
        this.#formatLine = formatLine;
    }

    async update(pages: MemberObject[][], currentPage: number): Promise<void> {
        if (pages.length > 0) {
            this.embed.setDescription(
                await this.formatDescription(pages[currentPage])
            );
            this.embed.setFooter({ text: `Page ${currentPage + 1}/${pages.length}` });
        }
        else {
            this.embed.setDescription('No data');
        }
    }

    async formatDescription(currentPage: MemberObject[]): Promise<string> {
        const deadMembers = [];
        const descriptionLines = [];

        for (const memberLine of currentPage) {
            const member = await DiscordUtil.getMember(this.#guild, memberLine.user_id);
            if (member) {
                descriptionLines.push(
                    this.#formatLine(member, memberLine)
                );
            }
            else {
                deadMembers.push(memberLine.user_id);
            }
        }

        if (deadMembers.length > 0)
            await this.#database.guildMembers.setDead(deadMembers);

        return descriptionLines.join('\n');
    }
}
