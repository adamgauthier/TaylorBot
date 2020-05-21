import WordArgumentType = require('../base/Word.js');
import ArgumentParsingError = require('../ArgumentParsingError.js');

enum Period {
    SevenDay = '7day',
    ThreeMonth = '3month',
    SixMonth = '6month',
    TwelveMonth = '12month',
    Overall = 'overall',
}

class LastFmPeriodArgumentType extends WordArgumentType {
    get id(): string {
        return 'last-fm-period';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default(): string {
        return Period.SevenDay;
    }

    parse(val: string): string {
        switch (val.trim().toLowerCase()) {
            case '7d':
            case '7day':
            case '7days':
                return Period.SevenDay;
            case '3m':
            case '3month':
            case '3months':
                return Period.ThreeMonth;
            case '6m':
            case '6month':
            case '6months':
                return Period.SixMonth;
            case '12m':
            case '12month':
            case '12months':
            case '1y':
            case '1year':
                return Period.TwelveMonth;
            case 'overall':
            case 'all':
                return Period.Overall;
            default:
                throw new ArgumentParsingError(
                    `Could not parse '${val}' into a valid Last.fm period. Use one of these: ${Object.values(Period).map(p => `\`${p}\``).join()}.`
                );
        }
    }
}

export = LastFmPeriodArgumentType;
