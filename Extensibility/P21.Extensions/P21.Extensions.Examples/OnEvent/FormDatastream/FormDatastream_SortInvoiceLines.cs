using P21.Extensions.BusinessRule;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Linq;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.OnEvent.FormDatastream
{
    /*
     * Rule Type: On Event
     * Event: Form Datastream Created
     * Event Type: Invoices
     * 
     * Description: This Rule is fired when the Form Datastream is created for invoices. It will look up a 
     *              user defined field for each item on the invoice and add it to the Datastream so that 
     *              information can be printed on the form. It will then also sort the invoice lines based
     *              on this new field.
     *              
     *              For the purpose of this example, the user defined field is simply called invoice_sort_group.
     */
    public class FormDatastream_SortInvoiceLines : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            /* 
             * During print preview, there will be multiple forms in the same datastream file. In this case we're going 
             * to loop through each form since we're going to retrieve the user defined field per invoice. When physically 
             * printing this will just loop once since there will be only 1 form per file. You could also get all lines in 
             * the entire file using Data.XMLDatastream.GetLines().
             */
            try
            {
                foreach (var form in Data.XMLDatastream.GetForms())
                {
                    // Get the invoice number for the form and populate the DataTable
                    var invoiceNo = Data.XMLDatastream.GetHeader(form).Element("INVOICE_NUMBER")?.Value;

                    using (var invoiceSortGroupData = GetInvoiceSortGroupData(invoiceNo))
                    {
                        // Loop through each line for the form and get the invoice_sort_group for that item
                        foreach (var line in Data.XMLDatastream.GetLines(form))
                        {
                            var invoiceLineNo = line.Element("LINE_NUMBER")?.Value;
                            var invoiceSortGroup = 9999;

                            if (invoiceSortGroupData != null)
                            {
                                if (invoiceSortGroupData.Rows.Count > 0)
                                {
                                    var invoiceSortLine = invoiceSortGroupData.Select($"invoice_no = '{invoiceNo}' AND line_no = {invoiceLineNo}");
                                    if (invoiceSortLine.Length == 1)
                                    {
                                        invoiceSortGroup = Convert.ToInt32(invoiceSortLine[0]["invoice_sort_group"]);
                                    }
                                }
                            }

                            // Add the user defined field for invoice_sort_group to the invoice lines.
                            line.Add(new XElement("INVOICE_SORT_GROUP", invoiceSortGroup));
                        }
                    }
                }

                // Sort all lines by the invoice_sort_group. By not passing the form as the first parameter, this will sort all 
                // forms by the given element. Also, pass true for the optional second parameter to indicate that we want to sort
                // in numeric order rather than alphabetic. Not specifying the optional 3rd parameter for sorting descending.
                Data.XMLDatastream.SortLines("INVOICE_SORT_GROUP", true);

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

        /// <summary>
        /// Returns a DataTable that is populated with the invoice sort group for each invoice line.
        /// </summary>
        private DataTable GetInvoiceSortGroupData(string invoiceNo)
        {
            var dt = new DataTable();

            try
            {
                // We can't use the P21SqlConnection for rules triggered on the 'Form Datastream Created' event
                // since those are triggered from outside of P21. So we have to create our own connection with
                // integrated security here.
                var connectionString = new SqlConnectionStringBuilder
                {
                    DataSource = Session.Server,
                    InitialCatalog = Session.Database,
                    UserID = Session.UserID,
                    IntegratedSecurity = true
                }.ConnectionString;

                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    var sql = GetInvoiceSortGroupSql(invoiceNo);

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddAndPersist($"Error retrieving invoice sort group info: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Returns the SQL necessary to retrieve the user defined field "invoice_sort_group" for each line on the given invoice.
        /// </summary>
        private static string GetInvoiceSortGroupSql(string invoiceNo)
        {
            var invoiceSortGroupSql = new StringBuilder();

            invoiceSortGroupSql.AppendLine("SELECT	p21_view_invoice_line.invoice_no");
            invoiceSortGroupSql.AppendLine("      , p21_view_invoice_line.line_no");
            // If no invoice_sort_group exists for the line, give it 9999 so it sorts to the bottom.
            invoiceSortGroupSql.AppendLine("      , COALESCE(inv_mast_ud.invoice_sort_group, 9999) invoice_sort_group");
            invoiceSortGroupSql.AppendLine("FROM	p21_view_invoice_line ");
            invoiceSortGroupSql.AppendLine("LEFT JOIN inv_mast_ud ON inv_mast_ud.inv_mast_uid = p21_view_invoice_line.inv_mast_uid ");
            invoiceSortGroupSql.AppendLine("WHERE	p21_view_invoice_line.invoice_no = " + invoiceNo);

            return invoiceSortGroupSql.ToString();
        }

        public override string GetName()
        {
            return "FormDatastream_SortInvoiceLines";
        }

        public override string GetDescription()
        {
            return "Adds a user defined field to invoice form datastream lines and sorts the lines based on it.";
        }
    }
}
