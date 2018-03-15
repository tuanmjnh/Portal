using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.IO;
namespace InvoiceTool
{
    /// <SUMMARY>
    /// This class validates an xml string or xml document against an xml
    /// schema.
    /// It has public methods that return a boolean value depending on 
    /// the validation
    /// of the xml.
    /// </SUMMARY>
    public class XmlSchemaValidator
    {
        private bool isValidXml = true;
        private string validationError = "";

        /// <SUMMARY>
        /// Empty Constructor.
        /// </SUMMARY>
        public XmlSchemaValidator()
        {

        }

        /// <SUMMARY>
        /// Public get/set access to the validation error.
        /// </SUMMARY>
        public String ValidationError
        {
            get
            {
                //return "<VALIDATIONERROR>" + this.validationError + "</VALIDATIONERROR>";
                return string.Format("<VALIDATIONERROR>{0}</VALIDATIONERROR>", this.validationError);
            }
            set
            {
                this.validationError = value;
            }
        }

        /// <SUMMARY>
        /// Public get access to the isValidXml attribute.
        /// </SUMMARY>
        public bool IsValidXml
        {
            get
            {
                return this.isValidXml;
            }
        }

        /// <SUMMARY>
        /// This method is invoked when the XML does not match
        /// the XML Schema.
        /// </SUMMARY>
        /// <PARAM name="sender"></PARAM>
        /// <PARAM name="args"></PARAM>
        private void ValidationCallBack(object sender,
                                   ValidationEventArgs args)
        {
            // The xml does not match the schema.
            isValidXml = false;
            this.ValidationError = args.Message;
        }


        /// <SUMMARY>
        /// This method validates an xml string against an xml schema.
        /// </SUMMARY>
        /// <PARAM name="xml">XML string</PARAM>
        /// <PARAM name="schemaNamespace">XML Schema Namespace</PARAM>
        /// <PARAM name="schemaUri">XML Schema Uri</PARAM>
        /// <RETURNS>bool</RETURNS>
        public bool ValidXmlDoc(string xml, string schemaNamespace, string schemaUri)
        {
            try
            {
                // Is the xml string valid?
                if (xml == null || xml.Length < 1)
                {
                    return false;
                }

                StringReader srXml = new StringReader(xml);
                return ValidXmlDoc(srXml, schemaNamespace, schemaUri);
            }
            catch (Exception ex)
            {
                this.ValidationError = ex.Message;
                this.isValidXml = false;
                
                return false;
            }
        }

        /// <SUMMARY>
        /// This method validates an xml document against an xml 
        /// schema.
        public bool ValidXmlDoc(XmlDocument xml,
               string schemaNamespace, string schemaUri)
        {
            try
            {
                // Is the xml object valid?
                if (xml == null)
                {
                    return false;
                }

                // Create a new string writer.
                StringWriter sw = new StringWriter();
                // Set the string writer as the text writer 
                // to write to.
                XmlTextWriter xw = new XmlTextWriter(sw);
                // Write to the text writer.
                xml.WriteTo(xw);
                // Get 
                string strXml = sw.ToString();

                StringReader srXml = new StringReader(strXml);

                return ValidXmlDoc(srXml, schemaNamespace, schemaUri);
            }
            catch (Exception ex)
            {
                this.ValidationError = ex.Message;
                this.isValidXml = false;
                return false;
            }
        }


        /// <SUMMARY>
        /// This method validates an xml string against an xml schema.
        /// </SUMMARY>
        /// <PARAM name="xml">StringReader containing xml</PARAM>
        /// <PARAM name="schemaNamespace">XML Schema Namespace</PARAM>
        /// <PARAM name="schemaUri">XML Schema Uri</PARAM>
        /// <RETURNS>bool</RETURNS>
        public bool ValidXmlDoc(StringReader xml,
               string schemaNamespace, string schemaUri)
        {
            // Continue?
            if (xml == null || schemaNamespace == null || schemaUri == null)
            {
                return false;
            }

            isValidXml = true;
            XmlValidatingReader vr;
            XmlTextReader tr;
            XmlSchemaCollection schemaCol = new XmlSchemaCollection();
            schemaCol.Add(schemaNamespace, schemaUri);

            try
            {
                // Read the xml.
                tr = new XmlTextReader(xml);
                // Create the validator.
                vr = new XmlValidatingReader(tr);
                // Set the validation type.
                vr.ValidationType = ValidationType.Auto;
                // Add the schema.
                if (schemaCol != null)
                {
                    vr.Schemas.Add(schemaCol);
                }
                // Set the validation event handler.
                vr.ValidationEventHandler +=
                   new ValidationEventHandler(ValidationCallBack);
                // Read the xml schema.
                while (vr.Read())
                {
                }

                vr.Close();

                return isValidXml;
            }
            catch (Exception ex)
            {
                this.ValidationError = ex.Message;
                this.isValidXml = false;
                return false;
            }
            finally
            {
                // Clean up...
                vr = null;
                tr = null;
            }
        }
    }
}