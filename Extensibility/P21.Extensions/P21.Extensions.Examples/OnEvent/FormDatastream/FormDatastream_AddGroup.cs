using P21.Extensions.BusinessRule;
using System;
using System.Linq;
using System.Xml.Linq;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.OnEvent.FormDatastream
{
    /*
     * Rule Type: On Event
     * Event: Form Datastream Created
     * Event Type: Invoices
     * 
     * Description: This Rule is fired when the Form Datastream is created for invoices. It simply adds a group
     *              to the header and line levels in the datastream file with hardcoded values. Typically, one
     *              would add a group when you have detail information for the header or line that could have
     *              multiple records per header or line.
     */
    public class FormDatastream_AddGroup : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            try
            {
                // This just shows how to get and modify the value of a detail group in the datastream.
                // Get the Carrier Group for the first header
                foreach (var carrierInfo in Data.XMLDatastream.GetGroup("CARRIERDEF", Data.XMLDatastream.GetHeaders().First()))
                {
                    // Change the XCARRIER_NAME element
                    if (carrierInfo != null)
                        carrierInfo.Element("XCARRIER_NAME").Value = "Modified Carrier Name";
                }

                // Add a group to the header level
                foreach (var hdr in Data.XMLDatastream.GetHeaders())
                {
                    var hdrTestGroup = new XElement("HDRTSTXDEF",
                        new XElement("TEST_TEXT", "Header Test Text"),
                        new XElement("INVOICE_NO", hdr.Element("INVOICE_NUMBER")?.Value));

                    Data.XMLDatastream.AddGroup(hdrTestGroup, hdr);
                }

                // Add a group to the line level
                foreach (var line in Data.XMLDatastream.GetLines())
                {
                    var lineTestGroup = new XElement("LINETSTDEF",
                        new XElement("TEST_TEXT", "Line Test Text"),
                        new XElement("ITEM_ID", line.Element("INVOICE_LINE_ITEM_ID")?.Value));

                    Data.XMLDatastream.AddGroup(lineTestGroup, line);
                }

                // Save the datastream back to the original file.
                Data.XMLDatastream.Document.Save(Data.XMLDatastream.FilePath);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;

                Log.AddAndPersist(ex.Message);
            }

            return result;
        }

        public override string GetName()
        {
            return "FormDatastream_AddGroup";
        }

        public override string GetDescription()
        {
            return "Adds a detail group to invoice headers and lines in the datastream.";
        }
    }
}
