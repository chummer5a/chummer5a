using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer.Classes
{
    class PDFParametersHelper
    {
        /// <summary>
        /// Generate a list of PDF Parameters from options.xml. 
        /// </summary>
        /// <returns></returns>
        public static List<ListItem<string>> GetListOfPDFParameters()
        {
            List<ListItem<string>> lstPdfParameters = new List<ListItem<string>>();

            XmlDocument objXmlDocument = XmlManager.Load("options.xml");

            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/chummer/pdfarguments/pdfargument");
            
            foreach (XmlNode objXmlNode in objXmlNodeList)
            {
                ListItem<string> objPDFArgument = new ListItem<string>(objXmlNode["value"].InnerText, objXmlNode["name"].InnerText);
                lstPdfParameters.Add(objPDFArgument);
            }

            return lstPdfParameters;
        }
    }
}
