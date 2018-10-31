using ChaT.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.bLogic
{
    public class AskMeCommon
    {

        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;

        public AskMeCommon(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public bool LogFailureResponse()
        {
            ChatFailureResponse fail = new ChatFailureResponse();
            try
            {
                fail.QuestionByUser = Message;
                fail.ParentId = Node;
                fail.Reviewed = false;
                fail.UpdatedDate = DateTime.UtcNow;
                db.ChatFailureResponse.Add(fail);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GoodBye()
        {
            if (Message.Contains("cya"))
                return true;
            else if (Message.Contains("good") && Message.Contains("bye"))
                return true;
            else if (Message.Contains("take") && Message.Contains("care"))
                return true;
            else
                return false;
        }

        public List<string> GetSuggestionList(int NewNode)
        {
            List<string> suggest = new List<string>();
            var hasSuggest = db.ChatIntent.Where(x => x.ParentId == NewNode && x.ChatIntentId > 2 & !x.IsRedirect).Select(y => y.IntentDescription);
            if (hasSuggest.Any())
            {
                suggest = hasSuggest.ToList();
            }
            else
            {
                suggest = db.ChatIntent.Where(x => x.ParentId == 0 && x.ChatIntentId > 2 & !x.IsRedirect).Select(y => y.IntentDescription).ToList();
            }

            return suggest;
        }

        public List<string> GetSuggestionList()
        {
            List<string> suggest = new List<string>();
            var hasSuggest = db.ChatIntent.Where(x => x.ParentId == Node && x.ChatIntentId > 2 & !x.IsRedirect).Select(y => y.IntentDescription);
            if (hasSuggest.Any())
            {
                suggest = hasSuggest.ToList();
            }
            else
            {
                suggest = db.ChatIntent.Where(x => x.ParentId == 0 && x.ChatIntentId > 2 & !x.IsRedirect).Select(y => y.IntentDescription).ToList();
            }

            return suggest;
        }

        public bool CheckEmptyMessage()
        {
            bool isEmpty = false;
            string message = Message;
            if (message.Length == 0)
            {
                isEmpty = true;
            }
            else if (message.Length > 0)
            {
                message = message.Trim();
                if (message.Length == 0)
                {
                    isEmpty = true;
                }
            }
            return isEmpty;
        }

    }
}