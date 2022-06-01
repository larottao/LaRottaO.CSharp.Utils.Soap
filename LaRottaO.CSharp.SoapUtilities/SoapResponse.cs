using System;

namespace LaRottaO.CSharp.SoapUtilities
{
    public class SoapResponse
    {
        public SoapResponse(bool success, int httpCode, string responseBody)
        {
            this.success = success;
            this.httpCode = httpCode;
            this.responseBody = responseBody;
        }

        public Boolean success { get; set; }
        public int httpCode { get; set; }
        public String responseBody { get; set; }
    }
}