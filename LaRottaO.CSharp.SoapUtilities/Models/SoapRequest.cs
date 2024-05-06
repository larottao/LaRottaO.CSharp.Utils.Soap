using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaRottaO.CSharp.SoapUtilities.Models
{
    public class SoapRequest
    {
        public String endpointUrl { get; set; }
        public List<String[]> headersList { get; set; }
        public StringBuilder xmlRequestBody { get; set; }
        public Boolean includeRequestOnResponse { get; set; }
        public String httpMethod { get; set; }
        public int timeout { get; set; }
        public Boolean showDebug { get; set; }

        public SoapRequest()

        {
            this.headersList = new List<String[]>();

            this.xmlRequestBody = new StringBuilder();

            this.includeRequestOnResponse = false;

            this.httpMethod = "POST";

            this.timeout = 40000;

            this.showDebug = true;
        }
    }
}