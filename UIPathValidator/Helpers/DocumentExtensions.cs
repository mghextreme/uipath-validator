using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace UIPathValidator
{
    public static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using(var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static bool IsInsideCommentOut(this XElement node, XmlNamespaceManager namespaces)
        {
            if (!namespaces.HasNamespace("ui"))
                return false;

            var ancestorComment = node.Ancestors(XName.Get("CommentOut", namespaces.LookupNamespace("ui")));
            return ancestorComment.Any();
        }
    }
}