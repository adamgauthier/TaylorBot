import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import GooglePlacesModule = require('../../modules/google/GooglePlacesModule.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class GooglePlaceArgumentType extends TextArgumentType {
    get id(): string {
        return 'google-place';
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<any> {
        const response = await GooglePlacesModule.findPlaceFromText(val);

        switch (response.status) {
            case 'OK':
                return response.candidates[0];
            case 'ZERO_RESULTS':
                throw new ArgumentParsingError(`Could not find '${val}' on Google Maps.`);
            default:
                throw new ArgumentParsingError(`Something went wrong when contacting Google Maps.`);
        }
    }
}

module.exports = GooglePlaceArgumentType;
