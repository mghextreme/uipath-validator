using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UIPathValidator
{
    public class XamlReader
    {
        public XmlNamespaceManager Namespaces { get; protected set; }

        public XDocument Document { get; protected set; }

        public XamlReader(string filePath)
        {
            using (TextReader textReader = File.OpenText(filePath))
            {
                Document = XDocument.Load(textReader);
                Namespaces = GetXmlNamespaces(Document.ToXmlDocument());
            }
        }

        private XmlNamespaceManager GetXmlNamespaces(XmlDocument xDoc)
        {
            XmlNamespaceManager result = new XmlNamespaceManager(xDoc.NameTable);
            result.AddNamespace(string.Empty, "http://schemas.microsoft.com/netfx/2009/xaml/activities");

            IDictionary<string, string> localNamespaces = null;
            XPathNavigator xNav = xDoc.CreateNavigator();
            while (xNav.MoveToFollowing(XPathNodeType.Element))
            {
                localNamespaces = xNav.GetNamespacesInScope(XmlNamespaceScope.Local);
                foreach (var localNamespace in localNamespaces)
                {
                    string prefix = localNamespace.Key;
                    if (string.IsNullOrEmpty(prefix))
                            prefix = "DEFAULT";

                    result.AddNamespace(prefix, localNamespace.Value);
                }
            }

            return result;
        }
    }
}