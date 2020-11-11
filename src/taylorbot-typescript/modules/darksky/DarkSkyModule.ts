import fetch = require('node-fetch');
import querystring = require('querystring');

import { EnvUtil } from '../util/EnvUtil';

const darkSkyApiKey = EnvUtil.getRequiredEnvVariable('TaylorBot_DarkSky__ApiKey');

type DarkSkyResponse = {
    currently: DarkSkyCurrently;
};

export type DarkSkyCurrently = {
    time: number;
    summary: string;
    icon: string;
    precipIntensity: number;
    precipProbability: number;
    precipType: string;
    temperature: number;
    apparentTemperature: number;
    dewPoint: number;
    humidity: number;
    pressure: number;
    windSpeed: number;
    windGust: number;
    windBearing: number;
    cloudCover: number;
    uvIndex: number;
    visibility: number;
    ozone: number;
};

export class DarkSkyModule {
    static async getCurrentForecast(lat: string, long: string): Promise<DarkSkyResponse> {
        const response = (await fetch(`https://api.darksky.net/forecast/${darkSkyApiKey}/${lat},${long}?${querystring.stringify({
            exclude: 'minutely,hourly,daily,alerts,flags',
            units: 'si'
        })}`).then(res => res.json())) as DarkSkyResponse;
        return response;
    }
}
