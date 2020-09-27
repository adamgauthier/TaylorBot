import fetch = require('node-fetch');

type HoroscopeResponse = {
    date: string;
    horoscope: string;
    sunsign: string;
}

export class HoroscopeModule {
    static async getDailyHoroscope(zodiacSign: string): Promise<HoroscopeResponse> {
        return (await fetch(
            `http://horoscope-api.herokuapp.com/horoscope/today/${zodiacSign}`
        ).then(res => res.json())) as HoroscopeResponse;
    }
}
