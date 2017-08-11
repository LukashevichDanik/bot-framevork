using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace bot1350.Services
{
    public class ImageService
    {
        public async Task GetImage(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var inboundMessage = await result as Activity;
                var outboundMessage = context.MakeMessage();
                var client = new HttpClient() { BaseAddress = new Uri("http://api.giphy.com") };
                var res = client.GetStringAsync("/v1/gifs/trending?api_key=dc6zaTOxFJmzC").Result;
                var data = ((dynamic)JObject.Parse(res)).data;
                var gif = data[(int)Math.Floor(new Random().NextDouble() * data.Count)];
                var gifUrl = gif.images.fixed_height.url.Value;
                var slug = gif.slug.Value;

                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(gifUrl);

                var image64 = "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes);

                outboundMessage.Attachments = new List<Attachment>();
                outboundMessage.Attachments.Add(new Attachment()
                {
                    ContentUrl = image64,
                    ContentType = "image/gif",
                    Name = slug + ".gif"
                });

                await context.PostAsync(outboundMessage);
            }
            catch
            {
                await context.PostAsync("Something went wrong =(");
            }
        }
    }
}