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

        public async Task<SoapResponse> executeRequest(String argEndpointUrl, List<String[]> argHeadersList, StringBuilder argXmlRequestBody, Boolean argIncludeRequestOnResponse = true, String argHttpMethod = "POST", int argTimeout = 40000, Boolean showDebug = true)
        {
          
                StringBuilder additionalData = new StringBuilder();

                HttpWebResponse myHttpWebResponse = null;
                StreamReader readStream = null;

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

                    httpWebRequest.CookieContainer = new CookieContainer();
                    httpWebRequest.KeepAlive = true;
                    httpWebRequest.Timeout = argTimeout;

                    /*******************************************************************/
                    // Additional headers the user may want to add
                    /*******************************************************************/

                    foreach (String[] header in argHeadersList)
                    {
                        additionalData.Append(additionalData + header[0] + ":" + header[1] + Environment.NewLine);
                        httpWebRequest.Headers.Add(header[0], header[1]);
                    }

                    Tuple<Boolean, String> insertSoapEnvelopeResult = InsertSoapEnvelopeIntoWebRequest(xmlEnelopeCreationResult.Item3, httpWebRequest);

                    if (!insertSoapEnvelopeResult.Item1)
                    {
                        return new SoapResponse(false, 500, insertSoapEnvelopeResult.Item2);
                    }
                    
                 
                    additionalData.Append(argXmlRequestBody.ToString() + Environment.NewLine + Environment.NewLine);
               

                    myHttpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    if (myHttpWebResponse.StatusCode != HttpStatusCode.OK)
                    {
                        return new SoapResponse(false, 500, additionalData + myHttpWebResponse.StatusDescription);
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

                    if (showDebug)
                    {
                        Console.WriteLine(additionalData + responseBody.ToString());
                    }

                    return new SoapResponse(true, 200, additionalData + responseBody.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(additionalData + " " + e.Message);
                    return new SoapResponse(true, 401, additionalData + " " + e.Message);
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