using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace bot1350.Services
{
    [Serializable]
    public class MotivationService
    {
        public async Task GetMotivation(IDialogContext context)
        {
            try
            {
                await context.PostAsync("Searching for new motivation.");
                var outMassage = context.MakeMessage();
                var client = new HttpClient() { BaseAddress = new Uri($"https://api.forismatic.com") };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("method", "getQuote"),
                    new KeyValuePair<string, string>("format", "json"),
                    new KeyValuePair<string, string>("key", ""),
                    new KeyValuePair<string, string>("lang", "ru")
                });
                var res = (await client.PostAsync($"/api/1.0/", content));
                string resultContent = await res.Content.ReadAsStringAsync();
                dynamic resultObject = ((dynamic)JObject.Parse(resultContent));
                string qText = resultObject.quoteText;
                string aText = resultObject.quoteAuthor;
                await context.PostAsync($"{qText}\n\n{aText}");
            }
            catch
            {
                await context.PostAsync("Something went wrong =(");
            }
        }
    }
}