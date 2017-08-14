using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using bot1350.Services;
using Newtonsoft.Json;
using System.IO;
using bot1350.DBServises;
using System.Collections.Generic;

namespace bot1350.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public int FromBody { get; private set; }
        private string _timerType { get; set; }
        private DateTime _timerDate { get; set; }
        private string _timerMessage { get; set; }
        private string _timerMessageType { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                PromptDialog.Choice(
                            context,
                            Choise,
                            new string[9] { "weather", "gif", "timer", "motivation", "cookie", "wish", "shopping", "news", "help" },
                            "Select action.");
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message + "\n\nType help to see more information.");
            }
        }

        public async Task Choise(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string text = await result;
                switch (text)
                {
                    case "weather":
                        PromptDialog.Text(
                            context,
                            AfterGetWeatherInfo,
                            "Sity?");
                        break;
                    case "gif":
                        await new ImageService().GetImage(context, result);
                        context.Wait(MessageReceivedAsync);
                        break;
                    case "help":
                        await HelpServise(context, result);
                        break;
                    case "timer":
                        PromptDialog.Text(
                            context,
                            AfterGetTimerInfoMessage,
                            "Select date.");
                        break;
                    case "motivation":
                        await new MotivationService().GetMotivation(context);
                        context.Wait(MessageReceivedAsync);
                        break;
                    case "cookie":
                        PromptDialog.Confirm(
                            context,
                            ResetCookie,
                            "Are you sure you want to reset the cookie?",
                            "Dont get you?!");
                        break;
                    case "wish":
                        PromptDialog.Text(
                            context,
                            AfterWishSelected,
                            "Enter the gift you want on the day of birth.");
                        break;
                    case "#wish admin":
                        await GetWishInfo(context);
                        break;
                    case "shopping":
                        PromptDialog.Text(
                            context,
                            AddShopingInfo,
                            "Specify shopping list.");
                        break;
                    case "#shopping admin":
                        PromptDialog.Text(
                            context,
                            GetShopingInfo,
                            "Specify period.");
                        break;
                    case "news":
                        PromptDialog.Choice(
                            context,
                            GetPopularNews,
                            new string[5] { "bbc-news", "bbc-sport", "cnn", "techcrunch", "newsweek" },
                            "Select sourse.");
                        break;
                    case "#card":
                        await GetCard(context);
                        break;
                    default:
                        await context.PostAsync("I can't get what you are asking for");
                        context.Wait(MessageReceivedAsync);
                        break;
                }
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message + "\n\nType help to see more information.");
            }
        }

        public async Task AfterGetWeatherInfo(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var sity = await result;
                var outMassage = context.MakeMessage();
                var client = new HttpClient() { BaseAddress = new Uri($"https://api.apixu.com") };
                var res = (await client.GetStringAsync($"/v1/forecast.json?key=b5c42416086f4feb8b671956171407&q={sity}"));
                var jsonObj = (dynamic)JObject.Parse(res);
                string temp = jsonObj.current.temp_c;
                string znak = Int32.Parse(temp) > 0 ? "+" : "";
                string resStr = $"In {sity} now {znak}{temp}\n\n";

                var forecast = jsonObj.forecast.forecastday[0];

                var maxTemp = forecast.day.maxtemp_c;
                var minTemp = forecast.day.mintemp_c;

                for (int i = 0; i < forecast.hour.Count; i++)
                {
                    string time = forecast.hour[i].time;
                    DateTime date = DateTime.Parse(time);
                    string znak1 = Int32.Parse(temp) > 0 ? "+" : "";
                    resStr += $"{date.ToShortTimeString()} - {znak1}{forecast.hour[i].temp_c}\n\n";
                }

                await context.PostAsync(resStr);

                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task HelpServise(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"#help - all functions;\n\n" +
                "#weather - weather in necessary sity;\n\n" +
                "#gif - see new awesome gif\n\n" +
                "#cookie - see new awesome gif\n\n");
            context.Wait(MessageReceivedAsync);
        }

        public async Task ResetCookie(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                bool userAnswer = await result;
                if (userAnswer)
                {

                    var res = new ResumptionCookie((Activity)context.Activity);
                    var data = JsonConvert.SerializeObject(res);
                    new FDH_BotService().AddUserCookie(context.Activity.From.Name, data);
                    await context.PostAsync($"Reseted cookie");
                }
                else
                {
                    await context.PostAsync($"Canceled");
                }
                context.Wait(MessageReceivedAsync);
            }
            catch
            {
                await context.PostAsync($"Something went wrong =(");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterGetTimerInfoType(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var choise = await result;
                PromptDialog.Text(
                            context,
                            AfterGetTimerInfoMessage,
                            "Select date.");
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterGetTimerInfoMessage(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string d = await result;
                DateTime date = DateTime.Parse(d);
                await context.PostAsync($"{date.ToShortDateString()} {date.ToShortTimeString()}");
                this._timerDate = date;
                PromptDialog.Text(
                        context,
                        AfterGetTimerInfoMessageFull,
                        "Enter your message");
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterGetTimerInfoMessageFull(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var d = await result;
                this._timerMessage = d;
                DateTime date;
                try
                {
                    date = context.Activity.LocalTimestamp.Value.DateTime;
                }
                catch (Exception e)
                {
                    date = DateTime.Now.ToUniversalTime();
                }
                await new TimerService().CustomTimer(context, result, date, this._timerDate, this._timerMessage);
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterWishSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string wish = await result;
                var res = new ResumptionCookie((Activity)context.Activity);
                var data = JsonConvert.SerializeObject(res);
                new FDH_BotService().AddDataToWishList(context.Activity.From.Name, wish, data);
                await context.PostAsync($"Saved.");
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task GetWishInfo(IDialogContext context)
        {
            try
            {
                var wishs = new FDH_BotService().GetUsersWish();
                await context.PostAsync(wishs);
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task GetShopingInfo(IDialogContext context, IAwaitable<string> result)
        {
            try
            {

                string dateStr = await result;
                DateTime date;
                if (dateStr.ToLower() == "all")
                {
                    date = new DateTime();
                }
                else
                {
                    date = DateTime.Parse(dateStr);
                }

                await context.PostAsync($"Getting shoping list from {date.ToShortDateString()}");
                var shopingList = new FDH_BotService().GetShopingItems(date);
                await context.PostAsync(shopingList);
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AddShopingInfo(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string shopingList = await result;
                var res = new ResumptionCookie((Activity)context.Activity);
                var data = JsonConvert.SerializeObject(res);
                new FDH_BotService().AddItemsToShopList(shopingList, context.Activity.From.Name, data);
                await context.PostAsync($"Saved.");
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task GetPopularNews(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var sourse = await result;
                var reply = context.MakeMessage();
                var client = new HttpClient() { BaseAddress = new Uri($"https://newsapi.org") };
                var res = (await client.GetStringAsync($"/v1/articles?source={sourse}&apiKey=36be0baaa96d469996549bfc4a2edcd2"));
                var jsonObj = (dynamic)JObject.Parse(res);
                var articles = jsonObj.articles;
                List<Attachment> list = new List<Attachment>();
                var count = articles.Count;
                var imag = articles[0].urlToImage;
                for (int i = 0; i < articles.Count; i++)
                {
                    list.Add(GetHeroCard(
                    articles[i].title.ToString(),
                    "",
                    articles[i].description.ToString(),
                    new CardImage(url: articles[i].urlToImage.ToString()),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: articles[i].url.ToString())));
                }

                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = list;

                await context.PostAsync(reply);

                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task GetCard(IDialogContext context)
        {
            try
            {
                var reply = context.MakeMessage();

                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = GetCardsAttachments();

                await context.PostAsync(reply);

                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
                context.Wait(MessageReceivedAsync);
            }
        }

        private static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "Azure Storage",
                    "Offload the heavy lifting of data center management",
                    "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
                    new CardImage(url: "https://docs.microsoft.com/en-us/azure/storage/media/storage-introduction/storage-concepts.png"),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: "https://azure.microsoft.com/en-us/services/storage/")),
                GetThumbnailCard(
                    "DocumentDB",
                    "Blazing fast, planet-scale NoSQL",
                    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
                    new CardImage(url: "https://docs.microsoft.com/en-us/azure/documentdb/media/documentdb-introduction/json-database-resources1.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
                GetHeroCard(
                    "Azure Functions",
                    "Process events with a serverless code architecture",
                    "An event-based serverless compute experience to accelerate your development. It can scale based on demand and you pay only for the resources you consume.",
                    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-5daae9212bb433ad0510fbfbff44121ac7c759adc284d7a43d60dbbf2358a07a/images/page/services/functions/01-develop.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
                GetThumbnailCard(
                    "Cognitive Services",
                    "Build powerful intelligence into your applications to enable natural and contextual interactions",
                    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
                    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-68b530dac63f0ccae8466a2610289af04bdc67ee0bfbc2d5e526b8efd10af05a/images/page/services/cognitive-services/cognitive-services.png"),
                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/cognitive-services/")),
            };
        }


        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }

        private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }
    }

}