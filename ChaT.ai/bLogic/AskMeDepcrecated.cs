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
    }
}