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
            await context.PostAsync("Type help to see more information.");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var activity = await result as Activity;
                var text = activity.Text.ToLower();
                switch (text)
                {
                    case "#weather":
                        PromptDialog.Text(
                            context,
                            AfterGetWeatherInfo,
                            "Sity?");
                        break;
                    case "#gif":
                        await new ImageService().GetImage(context, result);
                        context.Wait(MessageReceivedAsync);
                        break;
                    case "#help":
                        await HelpServise(context, result);
                        break;
                    case "#timer":
                        PromptDialog.Text(
                            context,
                            AfterGetTimerInfoMessage,
                            "Select date.");
                        break;
                    case "#motivation":
                        await new MotivationService().GetMotivation(context);
                        context.Wait(MessageReceivedAsync);
                        break;
                    case "#cookie":
                        PromptDialog.Confirm(
                            context,
                            ResetCookie,
                            "Are you sure you want to reset the cookie?",
                            "Dont get you?!");
                        break;
                    case "#wish":
                        PromptDialog.Text(
                            context,
                            AfterWishSelected,
                            "Enter the gift you want on the day of birth.");
                        break;
                    case "#wish admin":
                        await GetWishInfo(context);
                        break;
                    case "#shopping":
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
                        break;
                    default:
                        await context.PostAsync("I can't get what you are asking for");
                        context.Wait(MessageReceivedAsync);
                        break;
                }
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
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
                catch(Exception e)
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
    }
}