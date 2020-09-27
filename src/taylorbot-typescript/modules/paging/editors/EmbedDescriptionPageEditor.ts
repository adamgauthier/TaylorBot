import { EmbedPageEditor } from './EmbedPageEditor';

export class EmbedDescriptionPageEditor extends EmbedPageEditor<string> {
    async update(pages: string[], currentPage: number): Promise<void> {
        if (pages.length > 0) {
            this.embed.setDescription(
                await this.formatDescription(pages[currentPage])
            );
            this.embed.setFooter(`Page ${currentPage + 1}/${pages.length}`);
        }
        else {
            this.embed.setDescription('No data');
        }
    }

    formatDescription(currentPage: string): Promise<string> {
        return Promise.resolve(currentPage);
    }
}
