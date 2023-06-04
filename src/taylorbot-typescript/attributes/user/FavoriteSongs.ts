import { TextUserAttribute } from '../TextUserAttribute';
import { SimplePresenter } from '../user-presenters/SimplePresenter';

class FavoriteSongsAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'favoritesongs',
            aliases: ['fav', 'favsongs', 'favouritesongs'],
            description: 'favorite songs list',
            value: {
                label: 'fav',
                type: 'multiline-text',
                example: 'Taylor Swift - Enchanted, Coldplay - Amsterdam'
            },
            presenter: SimplePresenter,
            canList: false
        });
    }

    formatValue(attribute: Record<string, any>): string {
        return attribute.attribute_value;
    }
}

export = FavoriteSongsAttribute;
