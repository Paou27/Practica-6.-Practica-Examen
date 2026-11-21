using System;
using System.Collections.Generic;
using System.Web.Services;

namespace SegipSoap
{
    [WebService(Namespace = "http://segip.gob.bo/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class SegipService : WebService
    {
        private static readonly HashSet<string> CisValidos = new HashSet<string>
        {
            "1234567",
            "7654321",
            "4567890",
            "8901234",
            "5556677"
        };

        [WebMethod]
        public bool verificarCI(string ci)
        {
            if (string.IsNullOrWhiteSpace(ci))
            {
                return false;
            }
            return CisValidos.Contains(ci.Trim());
        }
    }
}
