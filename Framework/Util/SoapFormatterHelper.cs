using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;


namespace QCMonitor.Framework.Util {
    public class SoapFormatterHelper {
        public static string ObjectToSoapString(Object obj) {
            IFormatter formatter;
            MemoryStream memStream = null;
            string strObject = "";
            try {
                memStream = new MemoryStream();
                formatter = new SoapFormatter();
                formatter.Serialize(memStream, obj);
                strObject =
                   Encoding.UTF8.GetString(memStream.GetBuffer());

                //Check for the null terminator character

                int index = strObject.IndexOf("\0");
                if (index > 0) {
                    strObject = strObject.Substring(0, index);
                }
            } catch (Exception exception) {
                throw exception;
            } finally {
                if (memStream != null) memStream.Close();
            }
            return strObject;
        }

        public static Object SoapStringToObject(string soapString) {
            IFormatter formatter;
            MemoryStream memStream = null;
            Object objectFromSoap = null;
            try {
                //byte[] bytes = new byte[soapString.Length];

                var bytes = Encoding.UTF8.GetBytes(soapString);

                //Encoding.UTF8.GetBytes(soapString, 0,
                //             soapString.Length, bytes, 0);
                memStream = new MemoryStream(bytes);
                formatter = new SoapFormatter();
                objectFromSoap =
                     formatter.Deserialize(memStream);
            } catch (Exception exception) {
                throw exception;
            } finally {
                if (memStream != null) memStream.Close();
            }
            return objectFromSoap;
        }
    }
}
