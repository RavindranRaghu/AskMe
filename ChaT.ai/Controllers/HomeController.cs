using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChaT.db;
using System.Collections;
using AskMe.ai;
using ChaT.ai.bLogic;

namespace ChaT.ai.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private ChatDatabaseModel db;

        public HomeController()
        {
            db = new ChatDatabaseModel(); 
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Chat(string sender, string message)
        {
            AskMeChannel channel = new AskMeChannel(message);
            KeyValuePair<string, string> responseMessage = channel.ChatResponse();
            //ViewBag.Intent = responseMessage.Key;
            return Json(responseMessage.Value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Intent()
        {
            List<ChatIntent> intent = db.ChatIntent.ToList();
            return View(intent); 
        }

    }
}