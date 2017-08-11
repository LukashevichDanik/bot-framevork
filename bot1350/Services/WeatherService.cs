using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace bot1350.Services
{
    [Serializable]
    public class WeatherService
    {
        public async Task GetWeather(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                PromptDialog.Text(
                            context,
                            async (IDialogContext _context, IAwaitable<string> _result) =>
                            {
                                var sity = await _result;
                                await context.PostAsync($"Searching for new weather information in {sity}.");
                                var outMassage = context.MakeMessage();
                                var client = new HttpClient() { BaseAddress = new Uri($"https://api.apixu.com") };
                                var res = (await client.GetStringAsync($"/v1/current.json?key=b5c42416086f4feb8b671956171407&q={sity}"));
                                string temp = ((dynamic)JObject.Parse(res)).current.temp_c;
                                string znak = Int32.Parse(temp) > 0 ? "+" : "-";
                                outMassage.Text = $"In {sity} now {znak}{temp}";
                                await context.PostAsync(outMassage);
                            },
                            "What sity?");
            }
            catch
            {
                await context.PostAsync("Something went wrong =(");
            }
        }

        public async Task AfterfGetInfo(IDialogContext context, IAwaitable<string> result)
        {
            var sity = await result;
            await context.PostAsync($"Searching for new weather information in {sity}.");
            var outMassage = context.MakeMessage();
            var client = new HttpClient() { BaseAddress = new Uri($"https://api.apixu.com") };
            var res = (await client.GetStringAsync($"/v1/current.json?key=b5c42416086f4feb8b671956171407&q={sity}"));
            string temp = ((dynamic)JObject.Parse(res)).current.temp_c;
            string znak = Int32.Parse(temp) > 0 ? "+" : "-";
            outMassage.Text = $"In {sity} now {znak}{temp}";
            await context.PostAsync(outMassage);
        }
    }
}