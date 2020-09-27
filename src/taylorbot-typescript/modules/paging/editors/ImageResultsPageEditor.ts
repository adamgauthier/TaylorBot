import { URL } from 'url';
import { CustomSearchItem } from '../../google/GoogleImagesModule';
import { EmbedPageEditor } from './EmbedPageEditor';

export class ImageResultsPageEditor extends EmbedPageEditor<CustomSearchItem> {
    update(pages: CustomSearchItem[], currentPage: number): Promise<void> {
        const imageResult = pages[currentPage];

        const imageURL =
            ['http:', 'https:'].includes(new URL(imageResult.link).protocol) ?
                imageResult.link : imageResult.image.thumbnailLink;

        this.embed
            .setTitle(imageResult.title)
            .setURL(imageResult.image.contextLink)
            .setImage(imageURL);

        return Promise.resolve();
    }
}
