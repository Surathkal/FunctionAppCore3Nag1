using System;
using System.Xml;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using FunctionAppCore3Nag1.Models;

namespace FunctionAppCore3Nag1.Utility
{
    class XmlUtility
    {
        public static string ObjectToXmlString_second(Object _object, Type objType)
        {

            // Create and return an XmlSerializer instance used to
            // override and create SOAP messages.
            SoapAttributeOverrides mySoapAttributeOverrides = new SoapAttributeOverrides();
            SoapAttributes soapAtts = new SoapAttributes();

            // Override the SoapTypeAttribute.
            SoapTypeAttribute soapType = new SoapTypeAttribute();
            //soapType.TypeName = "Team";
            soapType.IncludeInSchema = false;
            //soapType.Namespace = "http://www.microsoft.com";

            // https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.soaptypeattribute.includeinschema?view=netframework-4.8
            // https://docs.microsoft.com/en-us/dotnet/standard/data/xml/
            // https://stackoverflow.com/questions/1729711/prevent-xmlserializer-from-emitting-xsitype-on-inherited-types/1730412#1730412
            // https://stackoverflow.com/questions/1729711/prevent-xmlserializer-from-emitting-xsitype-on-inherited-types/1730412
            // https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/dkwy2d72(v=vs.100)
            // https://csharp.hotexamples.com/examples/System.Xml/XmlWriterSettings/-/php-xmlwritersettings-class-examples.html

            //mySoapAttributeOverrides.Add(typeof(QuoteData), soapAtts);

            mySoapAttributeOverrides.Add(objType, soapAtts);


            // Serializes a class named Group as a SOAP message.  
            XmlTypeMapping myTypeMapping =
                new SoapReflectionImporter(mySoapAttributeOverrides).ImportTypeMapping(objType);


            XmlSerializer mySerializer = new XmlSerializer(myTypeMapping);
            //XmlSerializer mySerializer = new XmlSerializer(objType);

            XmlWriterSettings settings = new XmlWriterSettings();


            // this removes <?xml version="1.0" encoding="utf-16"?>
            //settings.ConformanceLevel = ConformanceLevel.Document;
            //settings.ConformanceLevel = ConformanceLevel.Auto;

            settings.OmitXmlDeclaration = true;

            settings.Indent = false;

            // this removes <?xml version="1.0" encoding="utf-16"?>
            //settings.OmitXmlDeclaration = true;

            settings.NewLineChars = string.Empty;
            settings.NewLineHandling = NewLineHandling.None;

            string xmlStr = string.Empty;

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    mySerializer.Serialize(xmlWriter, _object);

                    xmlStr = stringWriter.ToString();
                    xmlWriter.Close();
                }
                stringWriter.Close();
            }

            return xmlStr;
        }


        public static string ObjectToXmlString(Object _object, Type objType)
        {
            string xmlStr = string.Empty;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            //settings.OmitXmlDeclaration = true;
            //settings.ConformanceLevel = ConformanceLevel.Document;
            settings.NewLineChars = string.Empty;
            settings.NewLineHandling = NewLineHandling.None;


            /*
             // check all these attributes from the folloiwng URL
             https://csharp.hotexamples.com/examples/System.Xml/XmlWriterSettings/-/php-xmlwritersettings-class-examples.html

            var settings = new XmlWriterSettings
            {
                Indent = false,
                Encoding = new UTF8Encoding(false),
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                OmitXmlDeclaration = true,
                NewLineOnAttributes = false,
                DoNotEscapeUriAttributes = true
            };

            var settings = new XmlWriterSettings
             {
                 OmitXmlDeclaration = true,
                 ConformanceLevel = ConformanceLevel.Fragment,
                 Indent = true,
                 CloseOutput = false
             };

            */


            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    /*
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    */

                    //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });

                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

                    // this only removes namespace at root level
                    //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    //namespaces.Add(string.Empty, string.Empty);

                    XmlSerializer serializer = new XmlSerializer(objType);
                    serializer.Serialize(xmlWriter, _object, namespaces);

                    xmlStr = stringWriter.ToString();
                    xmlWriter.Close();
                }

                stringWriter.Close();
            }

            return xmlStr;
        }


        public static Stock GetStockDetails(HttpRequest req)
        {
            try
            {
                using (var streamReader = new StreamReader(req.Body))
                {
                    var xmlDoc = XDocument.Load(streamReader, LoadOptions.None);
                    //LoadAsync(streamReader, LoadOptions.None, CancellationToken.None);
                    return (Stock)new XmlSerializer(typeof(Stock)).Deserialize(xmlDoc.CreateReader());
                }
            } catch (XmlException e)
            {
                string type = e.GetType().ToString();

                throw e;
            }
        }
    }
}