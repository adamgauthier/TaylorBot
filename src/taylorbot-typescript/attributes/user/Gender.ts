import { TextUserAttribute } from '../TextUserAttribute';
import { SimplePresenter } from '../user-presenters/SimplePresenter';

class GenderAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'gender',
            aliases: [],
            description: 'gender',
            value: {
                label: 'gender',
                type: 'gender',
                example: 'female'
            },
            presenter: SimplePresenter,
            canList: false
        });
    }
}

export = GenderAttribute;
