import { TextUserAttribute } from '../TextUserAttribute';
import { SimpleImagePresenter } from '../user-presenters/SimpleImagePresenter';

class WaifuAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'waifu',
            aliases: [],
            description: 'waifu',
            value: {
                label: 'waifu',
                type: 'http-url',
                example: 'https://www.example.com/link/to/picture.jpg'
            },
            presenter: SimpleImagePresenter,
            canList: false
        });
    }
}

export = WaifuAttribute;
