using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;


namespace ChaT.ai.bLogic
{
    public class AskMeHiBye
    {

        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;

        public AskMeHiBye(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public bool Greet ()
        {
            if (Message.Contains("hello"))
                return true;
            else if (Message.Contains("good") && Message.Contains("morning"))
                return true;
            else if (Message.Contains("how") && Message.Contains("are"))
                return true;
            else if (Message.Contains("greeting"))
                return true;
            else if (Message.ToLower() == "hi")
                return true;
            else
                return false;
        }

        public bool GoodBye()
        {
            if (Message.Contains("cya"))
                return true;
            else if (Message.Contains("good") && Message.Contains("bye"))
                return true;
            else if (Message.Contains("take") && Message.Contains("care"))
                return true;
            else if (Message.Contains("thank"))
                return true;
            else
                return false;
        }

    }
}