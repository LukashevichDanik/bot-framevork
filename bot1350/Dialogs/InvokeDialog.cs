using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using bot1350.Services;

namespace bot1350.Dialogs
{
    [Serializable]
    public class InvokeDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Hi =) How can I help ypu?");
            context.Wait(new Dialogs.RootDialog().MessageReceivedAsync);
        }
    }
}