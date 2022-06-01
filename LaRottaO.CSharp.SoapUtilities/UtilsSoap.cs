using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LaRottaO.CSharp.SoapUtilities
{
    public class UtilsSoap
    {
        /// <summary>
        ///
        /// A wrapper for making SOAP requests
        ///
        /// 2021 06 14
        ///
        /// by Luis Felipe La Rotta, reusing code from S.O.
        ///
        /// </summary>
        ///

        public Task<SoapResponse> executeRequest(String argEndpointUrl, List<String[]> argHeadersList, StringBuilder argXmlRequestBody, Boolean argIncludeRequestOnResponse = true, String argHttpMethod = "POST")
        {
            return Task.Run(() =>
            {
                try
                {
                    Tuple<Boolean, String, XmlDocument> xmlEnelopeCreationResult = CreateSoapEnvelope(argXmlRequestBody.ToString());

                    if (!xmlEnelopeCreationResult.Item1)
                    {
                        return new SoapResponse(false, 400, xmlEnelopeCreationResult.Item2);
                    }

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(argEndpointUrl);

                    /*******************************************************************/
                    // Headers by default
                    /*******************************************************************/

                    httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    httpWebRequest.Accept = "text/xml";
                    httpWebRequest.Method = argHttpMethod;

                    /*******************************************************************/
                    // Additional headers the user may want to add
                    /*******************************************************************/

                    foreach (String[] header in argHeadersList)
                    {
                        httpWebRequest.Headers.Add(header[0], header[1]);
                    }

                    Tuple<Boolean, String> insertSoapEnvelopeResult = InsertSoapEnvelopeIntoWebRequest(xmlEnelopeCreationResult.Item3, httpWebRequest);

                    if (!insertSoapEnvelopeResult.Item1)
                    {
                        return new SoapResponse(false, 500, insertSoapEnvelopeResult.Item2);
                    }

                    IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(null, null);

                    asyncResult.AsyncWaitHandle.WaitOne();

                    using (WebResponse webResponse = httpWebRequest.EndGetResponse(asyncResult))

                    {
                        String responseBody = "";

                        using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                        {
                            responseBody = rd.ReadToEnd();
                        }

                        if (argIncludeRequestOnResponse)
                        {
                            responseBody = "<URL>" + Environment.NewLine + argEndpointUrl + Environment.NewLine + "</URL>" + Environment.NewLine + "<REQUEST>" + Environment.NewLine + argXmlRequestBody.ToString() + Environment.NewLine + "</REQUEST>" + Environment.NewLine + "<RESPONSE>" + Environment.NewLine + responseBody + Environment.NewLine + "</RESPONSE>";
                        }

                        return new SoapResponse(true, 200, responseBody);
                    }
                }
                catch (Exception ex)
                {
                    return new SoapResponse(false, 500, ex.ToString());
                }
            });
        }

        private static Tuple<Boolean, String, XmlDocument> CreateSoapEnvelope(String textoXmlRequest)
        {
            try
            {
                XmlDocument soapEnvelopeDocument = new XmlDocument();
                soapEnvelopeDocument.LoadXml(textoXmlRequest);
                return new Tuple<Boolean, String, XmlDocument>(true, "", soapEnvelopeDocument);
            }
            catch (Exception ex)
            {
                return new Tuple<Boolean, String, XmlDocument>(false, "Unable to create SOAP envelope from supplied XML Request body: " + ex.ToString(), null);
            }
        }

        private static Tuple<Boolean, String> InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            try
            {
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                return new Tuple<Boolean, String>(true, "");
            }
            catch (Exception ex)
            {
                return new Tuple<Boolean, String>(false, "Unable to Insert SOAP envelope into Web Request: " + ex.ToString());
            }
        }
    }
}