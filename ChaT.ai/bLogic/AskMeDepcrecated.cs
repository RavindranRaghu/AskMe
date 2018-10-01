using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.bLogic
{
    public class AskMeDepcrecated
    {
        //foreach (string vocab in vocabList)
        //{
        //    var hasIntent = intentList.Where(x => x.ParentId == Node).Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
        //    if (hasIntent.Any())
        //    {
        //        Dictionary<int, string> intentDict = hasIntent.Select(t => new { t.ChatIntentId, t.IntentName }).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
        //        foreach (KeyValuePair<int, string> intent in intentDict)
        //        {
        //            KeyValuePair<int, string> childIntent = GetChildIntent(intent.Key, vocabList);
        //            if (string.IsNullOrEmpty(childIntent.Value))
        //            {
        //                responseMessage = intentList.Where(x => x.ChatIntentId == intent.Key).Select(y => y.Response).FirstOrDefault();
        //                Node = intent.Key;
        //            }
        //            else
        //            {
        //                responseMessage = childIntent.Value;
        //                Node = childIntent.Key;
        //            }
        //            return new KeyValuePair<int, string>(Node, responseMessage);
        //        }
        //    }
        //}
        //Dictionary<int, string> intentNameDict = intentList.Select(t => new { t.ChatIntentId, t.IntentName }).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
        //LevenshteinDistance dist = new LevenshteinDistance();
        //foreach (string vocab in vocabList)
        //{
        //    foreach (KeyValuePair<int, string> intent in intentNameDict)
        //    {
        //        if (dist.Compute(vocab, intent.Value) < 4)
        //        {
        //            KeyValuePair<int, string> childIntent = GetChildIntent(intent.Key, vocabList);
        //            if (string.IsNullOrEmpty(childIntent.Value))
        //            {
        //                responseMessage = intentList.Where(x => x.ChatIntentId == intent.Key).Select(y => y.Response).FirstOrDefault();
        //                Node = intent.Key;
        //            }
        //            else
        //            {
        //                responseMessage = childIntent.Value;
        //                Node = childIntent.Key;
        //            }
        //            return new KeyValuePair<int, string>(Node, responseMessage);
        //        }
        //    }
        //}

        //KeyValuePair<int, string> childIntent = GetChildIntent(questionHighestMatch.Value, vocabList);
        //if (string.IsNullOrEmpty(childIntent.Value))
        //{
        //    responseMessage = response;
        //    Node = questionHighestMatch.Value;
        //}
        //else
        //{
        //    responseMessage = childIntent.Value;
        //    Node = childIntent.Key;
        //}

        //private KeyValuePair<int, string> GetChildIntent(int chatIntentId, List<string> vocabList)
        //{
        //    string entity = string.Empty;
        //    List<ChatIntent> intentList = db.ChatIntent.Where(z => z.ParentId == chatIntentId).ToList();

        //    foreach (string vocab in vocabList)
        //    {
        //        var hasIntent = intentList.Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
        //        if (hasIntent.Any())
        //        {
        //            Dictionary<int, string> intentNameDict = hasIntent.Select(t => new { t.ChatIntentId, t.Response }).ToList().ToDictionary(x => x.ChatIntentId, y => y.Response);
        //            foreach (KeyValuePair<int, string> intentName in intentNameDict)
        //            {
        //                return intentName;
        //            }
        //        }
        //    }

        //    List<string> intentNames = intentList.Select(x => x.IntentName).ToList();
        //    LevenshteinDistance dist = new LevenshteinDistance();
        //    foreach (string vocab in vocabList)
        //    {
        //        foreach (string intentName in intentNames)
        //        {
        //            if (dist.Compute(vocab, intentName) < 4)
        //            {
        //                //entity = entityName;
        //                //return entity;
        //            }
        //        }
        //    }

        //    return new KeyValuePair<int, string>();
        //}



        //ChatIntent nodeDetail = db.ChatIntent.Where(x => x.ChatIntentId == node).FirstOrDefault();
        //if (nodeDetail.IntentName == "askpaymentspecialist")
        //{
        //    Session["phone"] = message;
        //    nodeDetail = db.ChatIntent.Where(x => x.ParentId == node).FirstOrDefault();
        //    finalResponse = nodeDetail.Response;
        //    node = nodeDetail.ChatIntentId;
        //}
        //else if (nodeDetail.IntentName == "askphonenumber")
        //{
        //    nodeDetail = db.ChatIntent.Where(x => x.ParentId == node).FirstOrDefault();
        //    if (Session["phone"] != null)
        //        phoneNumber = Session["phone"].ToString();
        //    else
        //        phoneNumber = "+1 980 233 7575";

        //    finalResponse = nodeDetail.Response;
        //    finalResponse = finalResponse.Replace("paramphonenumber", phoneNumber);
        //    finalResponse = finalResponse.Replace("paramappointmenttime", message);
        //    node = nodeDetail.ChatIntentId;
        //}
    }
}