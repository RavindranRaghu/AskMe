using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChaT.db;
using System.Collections;
using AskMe.ai;
using ChaT.ai.bLogic;
using System.IO;
using ChaT.ai.Dto;

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
            ViewBag.nodeLevel = 0;
            List<ChatIntent> intentList = db.ChatIntent.Where(x => x.ParentId == 0 & x.ChatIntentId > 2).ToList();
            return View(intentList);
        }

        public ActionResult Chat(string sender, string message, int node)
        {
            AskMeChannel channel = new AskMeChannel(message, node);
            KeyValuePair<int, string> responseMessage = channel.ChatInitializer();
            node = responseMessage.Key;
            List<string> suggest = new List<string>();
            var hasSuggest = db.ChatIntent.Where(x => x.ParentId == node && x.ChatIntentId > 2).Select(y => y.IntentName);
            if (hasSuggest.Any())
            {
                suggest = hasSuggest.ToList();
            }
            else
            {
                suggest = db.ChatIntent.Where(x => x.ParentId == 0 && x.ChatIntentId > 2 ).Select(y => y.IntentName).ToList();
            }
            var result = new { node = node, response = responseMessage.Value, suggest = suggest };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Intent()
        {
            List<ChatIntent> intent = db.ChatIntent.ToList();
            return View(intent); 
        }

        public ActionResult Voice()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadAudio(AudioContent content)
        {
            if (content != null)
            {
                byte[] bytes = Convert.FromBase64String(content.contentAsBase64String);
            }
            return RedirectToAction("voice");
        }
    }
}