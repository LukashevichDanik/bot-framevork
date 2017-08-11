using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace bot1350.Services
{
    public class SheduleService
    {
        private int _time;
        private string _text;

        public async Task GetInfo(IDialogContext context, IAwaitable<object> result)
        {
            try
            {

                await context.PostAsync("Fill time start");
                //context.Call<object>(GetText, this.GetInfo);
            }
            catch
            {
                await context.PostAsync("Something went wrong =(");
            }
        }

        public async Task GetText(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                this._text = (await result as Activity).Text;
                await context.PostAsync($"You post {this._text}");
            }
            catch
            {
                await context.PostAsync("Something went wrong =(");
            }
        }
    }
}