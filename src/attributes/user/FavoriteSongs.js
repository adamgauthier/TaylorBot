'use strict';

const SimpleUserTextAttribute = require('../SimpleUserTextAttribute.js');

class FavoriteSongsAttribute extends SimpleUserTextAttribute {
    constructor() {
        super({
            id: 'favoritesongs',
            aliases: ['fav', 'favsong', 'favsongs', 'favouritesongs'],
            description: 'favorite songs list',
            value: {
                label: 'fav',
                type: 'multiline-text',
                example: 'Taylor Swift - Enchanted, Coldplay - Amsterdam'
            }
        });
    }
}

module.exports = FavoriteSongsAttribute;