'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');
const SimpleImagePresentor = require('../presentors/SimpleImagePresentor.js');

class WaifuAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'waifu',
            description: 'waifu',
            value: {
                label: 'waifu',
                type: 'http-url',
                example: 'https://www.example.com/link/to/picture.jpg'
            },
            presentor: SimpleImagePresentor
        });
    }
}

module.exports = WaifuAttribute;