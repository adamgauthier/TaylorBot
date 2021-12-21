import fetch = require('node-fetch');
import moment = require('moment');

type HoroscopeResponse = {
    date: moment.Moment;
    horoscope: string;
    sunsign: string;
}

export class HoroscopeModule {
    static async getDailyHoroscope(zodiacSign: string): Promise<HoroscopeResponse> {
        const body = await fetch(
            `https://www.ganeshaspeaks.com/horoscopes/daily-horoscope/${zodiacSign}/`
        ).then(res => res.text());

        const horoscope = body.match(/<p id="horo_content">(.*)<\/p>/)![1];
        const dateString = body.match(/<p class="mb-0">(.*)<\/p>/)![1];

        const date = moment.utc(dateString, 'DD-MM-YYYY');

        return { date, horoscope, sunsign: zodiacSign };
    }
}
