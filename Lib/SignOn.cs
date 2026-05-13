using System;
using System.Xml;

namespace OfxSharpLib
{
    public class SignOn
    {
        public string StatusSeverity { get; set; }

        public DateTime DtServer { get; set; }

        public int StatusCode { get; set; }

        public string Language { get; set; }

        public string IntuBid { get; set; }

        public SignOn(XmlNode node)
        {
            int.TryParse(node.GetValue("//CODE"), out var code);
            StatusCode = code;
            StatusSeverity = node.GetValue("//SEVERITY");
            DtServer = node.GetValue("//DTSERVER").ToDate();
            Language = node.GetValue("//LANGUAGE");
            IntuBid = node.GetValue("//INTU.BID");
        }
    }
}