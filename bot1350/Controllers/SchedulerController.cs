using bot1350.DBServises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace bot1350.Controllers
{
    public class SchedulerController : ApiController
    {
        [HttpGet]
        public void Response()
        {
            new FDH_BotService().AddDataToLog();
        }
    }
}
