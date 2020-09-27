import { EmbedPageEditor } from './EmbedPageEditor';
import { StringUtil } from '../../util/StringUtil';
import { UrbanResult } from '../../urban/UrbanDictionaryModule';

export class UrbanResultsPageEditor extends EmbedPageEditor<UrbanResult> {
    update(pages: UrbanResult[], currentPage: number): Promise<void> {
        const result = pages[currentPage];

        this.embed.fields.pop();

        this.embed
            .setTitle(result.word)
            .setURL(result.permalink)
            .setTimestamp(new Date(Date.parse(result.written_on)))
            .setDescription(StringUtil.shrinkString(result.definition, 2048, ' (...)'))
            .setFooter(result.author)
            .addField('Votes', `👍 \`${result.thumbs_up}\` | \`${result.thumbs_down}\` 👎`, true);

        return Promise.resolve();
    }
}
