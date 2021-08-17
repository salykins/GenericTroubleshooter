using System;
using System.Collections.Generic;

namespace GenericTroubleshooting.HttpFunctions.Classes
{
    public class BaseHttpRequest
    {
        public string ClientId { get; set; }
        public string Payload { get; set; }
        public List<string> PurposeIds { get; set; }
        public string DataSubjectId { get; set; }
        public string PersonalDataRegion { get; set; }
    }
}
