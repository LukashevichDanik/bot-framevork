﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace bot1350
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else if (activity.Type == ActivityTypes.Invoke)
            {
                var res = new ResumptionCookie(activity);
                var data = JsonConvert.SerializeObject(res);

                File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/cookie.json"), data);
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        //public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        //{
        //    var res = new ResumptionCookie(activity);
        //    var data = JsonConvert.SerializeObject(res);

        //    File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/cookie.json"), data);

        //    //var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
        //    //await connector.Conversations.ReplyToActivityAsync(activity.CreateReply("I'll get right onto thet"));

        //    await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    return response;
        //}

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Invoke)
            {

            }

            return null;
        }
    }
}