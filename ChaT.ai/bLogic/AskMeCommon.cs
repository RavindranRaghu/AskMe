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

    }
}