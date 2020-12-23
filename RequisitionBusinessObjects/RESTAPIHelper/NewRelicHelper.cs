using System.Collections.Generic;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    public class NewRelicHelper
    {
        public void logNewRelicEvents(string eventname, string key, string values, string eventType)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("eventname", eventname);
            eventAttributes.Add("key", key);
            eventAttributes.Add("values", values);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventType, eventAttributes);
        }
    }
}
