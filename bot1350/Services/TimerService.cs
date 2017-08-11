using bot1350.DBServises;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace bot1350.Services
{
    public class TimerService
    {

        private System.Timers.Timer _timer { get; set; }
        private IDialogContext _context { get; set; }
        private string _timerMessage { get; set; }
        private string _timerMessageType { get; set; }
        private string _timerType { get; set; }
        private string userName { get; set; }

        public async Task CustomTimer(IDialogContext context, IAwaitable<object> result, DateTime currentDate, DateTime selectedDate, string message)
        {
            try
            {
                var res = new ResumptionCookie((Activity)context.Activity);
                var data = JsonConvert.SerializeObject(res);
                new FDH_BotService().AddUserCookie(context.Activity.From.Name, data);

                double milisecs = (selectedDate.ToUniversalTime() - currentDate.ToUniversalTime()).TotalMilliseconds;



                this._timerMessage = message;
                this.userName = context.Activity.From.Name;
                if (milisecs > 0)
                {
                    this._context = context;
                    await context.PostAsync("Done");
                    this._timer = new System.Timers.Timer(milisecs);
                    this._timer.Elapsed += TimerCallbackCustom;
                    this._timer.Enabled = true;
                }
                else
                {
                    await context.PostAsync("Selected old date.");
                }
            }
            catch (Exception e)
            {
                await context.PostAsync(e.Message);
            }
        }

        public void TimerCallbackCustom(object source, ElapsedEventArgs e)
        {
            try
            {
                var resumeJson = new FDH_BotService().GetUserCookie(this.userName).UserCookie;
                dynamic resumeData = JsonConvert.DeserializeObject(resumeJson);

                string botId = resumeData.address.botId;
                string channelId = resumeData.address.channelId;
                string userId = resumeData.address.userId;
                string conversationId = resumeData.address.conversationId;
                string serviceUrl = resumeData.address.serviceUrl;
                string userName = resumeData.userName;
                bool isGroup = resumeData.isGroup;

                var resume = new ResumptionCookie(new Address(botId, channelId, userId, conversationId, serviceUrl), userName, isGroup, "en");

                var messageActyvity = (Activity)resume.GetMessage();
                var replay = messageActyvity.CreateReply();

                replay.Text = this._timerMessage;
                var client = new ConnectorClient(new Uri(messageActyvity.ServiceUrl));
                client.Conversations.ReplyToActivity(replay);
                this._timer.Dispose();
            }
            catch (Exception e1)
            {
                this._timer.Dispose();
            }

            //this._timer.Dispose();
        }

        void run()
        {
            CancellationTokenSource cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            RepeatActionEvery(() => Console.WriteLine("Action"), TimeSpan.FromSeconds(1), cancellation.Token).Wait();
            Console.WriteLine("Finished action loop.");
        }

        public static async Task RepeatActionEvery(Action action, TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                action();
                Task task = Task.Delay(interval, cancellationToken);

                try
                {
                    await task;
                }

                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
    }
}