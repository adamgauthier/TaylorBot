'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');

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
            }
        });
    }
}

module.exports = FavoriteSongsAttribute;