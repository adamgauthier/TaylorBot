'use strict';

const fetch = require('node-fetch');

class HoroscopeModule {
    static getDailyHoroscope(zodiacSign) {
        return fetch(`http://horoscope-api.herokuapp.com/horoscope/today/${zodiacSign}`).then(res => res.json());
    }
}

module.exports = HoroscopeModule;