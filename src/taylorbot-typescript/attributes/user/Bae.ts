import { TextUserAttribute } from '../TextUserAttribute';
import { SimplePresenter } from '../user-presenters/SimplePresenter';

class BaeAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'bae',
            aliases: [],
            description: 'bae',
            value: {
                label: 'bae',
                type: 'text',
                example: 'Taylor Swift'
            },
            presenter: SimplePresenter,
            canList: false
        });
    }
}

export = BaeAttribute;
