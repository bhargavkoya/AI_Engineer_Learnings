using ModelContextProtocol;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherSearchMcpServer.Extensions;

namespace WeatherSearchMcpServer.Tools
{
    [McpServerToolType]
    public sealed class WeatherTools
    {
        // GetAlerts: queries the National Weather Service alerts API
        [McpServerTool, Description("Get active weather alerts for a US state.")]
        public static async Task<string> GetAlerts(
            HttpClient client,
            [Description("Two-letter US state abbreviation, e.g. NY, TX, CA.")] string state)
        {
            try
            {
                using var doc = await client.ReadJsonDocumentAsync($"/alerts/active/area/{state}");
                var alerts = doc.RootElement.GetProperty("features").EnumerateArray().ToList();

                if (alerts.Count == 0)
                    return $"No active weather alerts for {state}.";

                return string.Join("\n---\n", alerts.Take(5).Select(a =>
                {
                    var p = a.GetProperty("properties");
                    return $"""
                    Event:    {p.GetProperty("event").GetString()}
                    Area:     {p.GetProperty("areaDesc").GetString()}
                    Severity: {p.GetProperty("severity").GetString()}
                    """;
                }));
            }
            catch (HttpRequestException ex)
            {
                // Return a tool-level error - don't throw, let the agent handle it gracefully
                return $"Could not retrieve alerts for {state}: {ex.Message}";
            }
        }

        // GetForecast: resolves lat/lon to a NWS grid point, then fetches the forecast
        [McpServerTool, Description("Get a weather forecast for a geographic location.")]
        public static async Task<string> GetForecast(
            HttpClient client,
            [Description("Latitude in decimal degrees, e.g. 40.7128.")] double latitude,
            [Description("Longitude in decimal degrees, e.g. -74.0060.")] double longitude)
        {
            try
            {
                // NWS requires coordinates in invariant culture format (no locale-specific decimal separator)
                var pointUrl = string.Create(
                    CultureInfo.InvariantCulture, $"/points/{latitude},{longitude}");

                using var locationDoc = await client.ReadJsonDocumentAsync(pointUrl);
                var forecastUrl = locationDoc.RootElement
                    .GetProperty("properties")
                    .GetProperty("forecast")
                    .GetString()
                    ?? throw new McpException("NWS did not return a forecast URL.");

                using var forecastDoc = await client.ReadJsonDocumentAsync(forecastUrl);
                var periods = forecastDoc.RootElement
                    .GetProperty("properties")
                    .GetProperty("periods")
                    .EnumerateArray()
                    .Take(3); // return next 3 forecast periods

                return string.Join("\n---\n", periods.Select(p => $"""
                {p.GetProperty("name").GetString()}
                Temp: {p.GetProperty("temperature").GetInt32()}°F
                Wind: {p.GetProperty("windSpeed").GetString()} {p.GetProperty("windDirection").GetString()}
                {p.GetProperty("shortForecast").GetString()}
                """));
            }
            catch (Exception ex) when (ex is not McpException)
            {
                return $"Could not retrieve forecast for ({latitude}, {longitude}): {ex.Message}";
            }
        }
    }
}
