﻿namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public interface IForecastResult { }
public record CurrentForecast(string Summary, double TemperatureCelsius, double WindSpeed, double Humidity, long Time, string? IconUrl) : IForecastResult;
public record WeatherGenericErrorResult() : IForecastResult;

public interface IWeatherClient
{
    ValueTask<IForecastResult> GetCurrentForecastAsync(string latitude, string longitude);
}
