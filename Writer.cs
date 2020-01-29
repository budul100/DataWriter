﻿using DataWriter.Models;
using System.Xml;
using System.Xml.Serialization;

namespace DataWriter
{
    public class Writer<T>
    {
        #region Private Fields

        private readonly object content;
        private readonly XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        private readonly XmlAttributeOverrides overrides;
        private readonly XmlRootAttribute root;
        private readonly string rootNamespace;

        #endregion Private Fields

        #region Public Constructors

        public Writer
            (T content, string rootElement, string rootNamespace)
        {
            this.content = content;
            this.rootNamespace = rootNamespace;

            root = GetRootAttribute(rootElement);
            overrides = GetOverrides(root);
        }

        public Writer(T content) :
            this(content, null, null)
        { }

        #endregion Public Constructors

        #region Public Methods

        public void AddNamespace(string prefix, string ns)
        {
            namespaces.Add(
                prefix: prefix,
                ns: ns);
        }

        public string Get(bool withoutXmlHeader = false)
        {
            return GetAsString(withoutXmlHeader);
        }

        #endregion Public Methods

        #region Private Methods

        private static XmlAttributeOverrides GetOverrides(XmlRootAttribute root)
        {
            var result = new XmlAttributeOverrides();

            if (root != null)
            {
                var attributes = new XmlAttributes
                {
                    XmlRoot = root
                };

                result = new XmlAttributeOverrides();
                result.Add(
                    type: typeof(T),
                    attributes: attributes);
            }

            return result;
        }

        private string GetAsString(bool withoutXmlHeader = false)
        {
            var result = default(string);

            var xmlSerializer = GetSerializer();

            using (var textWriter = new UTF8Writer())
            {
                if (withoutXmlHeader)
                {
                    using (var fragementWriter = new XmlFragmentWriter(textWriter))
                    {
                        fragementWriter.Formatting = Formatting.Indented;

                        xmlSerializer.Serialize(
                            xmlWriter: fragementWriter,
                            o: this.content,
                            namespaces: namespaces);
                        result = fragementWriter.ToString();
                    }
                }
                else
                {
                    xmlSerializer.Serialize(
                        textWriter: textWriter,
                        o: this.content,
                        namespaces: namespaces);
                    result = textWriter.ToString();
                }
            }

            return result;
        }

        private XmlRootAttribute GetRootAttribute(string rootElement)
        {
            var result = default(XmlRootAttribute);

            if (!string.IsNullOrWhiteSpace(rootNamespace))
            {
                result = new XmlRootAttribute();

                AddNamespace(
                    prefix: string.Empty,
                    ns: rootNamespace);

                result.Namespace = rootNamespace;
            }

            if (!string.IsNullOrWhiteSpace(rootElement))
            {
                if (result == null)
                    result = new XmlRootAttribute();
                result.ElementName = rootElement;
            }

            return result;
        }

        private XmlSerializer GetSerializer()
        {
            var result =
                overrides != null ?
                new XmlSerializer(
                    type: content.GetType(),
                    overrides: overrides,
                    extraTypes: null,
                    root: root,
                    defaultNamespace: rootNamespace) :
                new XmlSerializer(content.GetType());

            return result;
        }

        #endregion Private Methods
    }
}