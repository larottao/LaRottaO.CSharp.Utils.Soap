using LaRottaO.CSharp.SoapUtilities.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public SoapResponse executeRequest(SoapRequest request)
        {
            StringBuilder debugData = new StringBuilder();

            HttpWebResponse myHttpWebResponse = null;
            StreamReader readStream = null;

            try
            {
                Tuple<Boolean, String, XmlDocument> xmlEnelopeCreationResult = createSoapEnvelope(request.xmlRequestBody.ToString());

                if (!xmlEnelopeCreationResult.Item1)
                {
                    return new SoapResponse(false, 400, xmlEnelopeCreationResult.Item2);
                }

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(request.endpointUrl);

                /*******************************************************************/
                // Headers by default
                /*******************************************************************/

                httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
                httpWebRequest.Accept = "text/xml";
                httpWebRequest.Method = request.httpMethod;

                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.KeepAlive = true;
                httpWebRequest.Timeout = request.timeout;

                /*******************************************************************/
                // Additional headers the user may want to add
                /*******************************************************************/

                foreach (String[] header in request.headersList)
                {
                    httpWebRequest.Headers.Add(header[0], header[1]);
                    debugData.Append(debugData + header[0] + ":" + header[1] + Environment.NewLine);
                }

                Tuple<Boolean, String> insertSoapEnvelopeResult = InsertSoapEnvelopeIntoWebRequest(xmlEnelopeCreationResult.Item3, httpWebRequest);

                if (!insertSoapEnvelopeResult.Item1)
                {
                    return new SoapResponse(false, 500, insertSoapEnvelopeResult.Item2);
                }

                debugData.Append(request.xmlRequestBody.ToString() + Environment.NewLine + Environment.NewLine);

                myHttpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (myHttpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    return new SoapResponse(false, 500, debugData + myHttpWebResponse.StatusDescription);
                }

                StringBuilder responseBody = new StringBuilder();

                Stream receiveStream = myHttpWebResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                readStream = new StreamReader(receiveStream, encode);

                Char[] read = new Char[256];

                int count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    String str = new String(read, 0, count);
                    responseBody.Append(str);
                    count = readStream.Read(read, 0, 256);
                }

                if (request.showDebug)
                {
                    Debug.WriteLine(debugData + responseBody.ToString());
                }

                return new SoapResponse(true, 200, debugData + responseBody.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(debugData + " " + e.Message);
                return new SoapResponse(false, 401, debugData + " " + e.Message);
            }
            finally
            {
                if (myHttpWebResponse != null)
                {
                    myHttpWebResponse.Close();
                }

                if (readStream != null)
                {
                    readStream.Close();
                }
            }
        }

        private static Tuple<Boolean, String, XmlDocument> createSoapEnvelope(String textoXmlRequest)
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