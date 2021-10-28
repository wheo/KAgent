using System;
using System.Collections.Generic;
using System.Xml;

namespace KAgent.Util
{
    internal class MakeXML
    {
        private String filename;
        private String path;
        private String fullpath;

        private XmlDocument m_xmlDoc;

        private String m_xmlString;

        private String m_base64xml;

        public String getXmlString()
        {
            if (m_xmlString == "")
            {
                return "";
            }
            else
            {
                return m_xmlString;
            }
        }

        public String getbase64XmlString()
        {
            if (m_base64xml == "")
            {
                return "";
            }
            else
            {
                return m_base64xml;
            }
        }

        public MakeXML(Dictionary<String, String> map)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode root = xmlDoc.CreateElement("contentinfo");
            XmlNode sTag;
            XmlCDataSection CData;

            sTag = xmlDoc.CreateElement("contentid");
            sTag.InnerText = map["contentid"];

            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("cornerid");
            sTag.InnerText = map["cornerid"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("programid");
            sTag.InnerText = map["programid"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("originid");
            sTag.InnerText = map["originid"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("contentnumber");
            sTag.InnerText = map["contentnumber"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("cornernumber");
            sTag.InnerText = map["cornernumber"];
            root.AppendChild(sTag);

            //sTag = xmlDoc.CreateElement("preview");
            //sTag.InnerText = map["preview"];
            //root.AppendChild(sTag);

            CData = xmlDoc.CreateCDataSection(map["preview"]);
            root.AppendChild(CData);

            sTag = xmlDoc.CreateElement("broaddate");
            sTag.InnerText = map["broaddate"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("title");
            sTag.InnerText = map["title"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("contentimg");
            sTag.InnerText = map["contentimg"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("searchkeyword");
            sTag.InnerText = map["searchkeyword"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("regdate");
            sTag.InnerText = map["regdate"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("modifydate");
            sTag.InnerText = map["modifydate"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("actor");
            sTag.InnerText = map["actor"];
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("isuse");
            sTag.InnerText = "Y";
            root.AppendChild(sTag);

            sTag = xmlDoc.CreateElement("addfield");
            sTag.InnerText = "addfield_test";
            root.AppendChild(sTag);

            xmlDoc.AppendChild(root);

            m_xmlDoc = xmlDoc;
        }

        private bool verifingXML()
        {
            return true;
        }

        public void setPath(String path)
        {
            this.path = path;
        }

        public void setFilename(String filename)
        {
            this.filename = filename;
        }

        public bool MakeFile(String filename)
        {
            m_xmlDoc.Save(filename);
            return true;
        }

        public bool base64Encoding()
        {
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(m_xmlString);
            m_base64xml = System.Convert.ToBase64String(arr);
            return true;
        }

        public bool MakeXmlToString()
        {
            m_xmlString = m_xmlDoc.OuterXml;
            return true;
        }
    }
}