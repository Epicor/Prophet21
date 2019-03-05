using P21.Extensions.BusinessRule;
using System;
using System.Data;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.OnDemand
{
    /*
     * Window: Order Entry
     * Setup - Order Tab: 
     *  1.  Add user-defined field for discount percent
     *      name: discount_pct (system will name ufc_oe_hdr_ud_discount_pct)
     *      datetype: decimal
     *      decimal percision: 2
     *  2.  Add button
     *      name: system will name cb_usersd1 or something similar
     *      text: Add New Row for Discount
     *      
     * Trigger: Order Header (Order tab) user defined button added above (cb_usersd1)
     * Rule Type: On Demand
     * Multi-Row:  Checked
     * 
     * Fields passed to the rule:
     *		d_oe_header
     *			ufc_oe_hdr_ud_discount_pct (alias = discount_pct)
     *			cb_usersd1 (alias = add_discount_button)
     *		d_dw_oe_line_dataentry
     *			oe_order_item_id (alias = item_id)
     *			unit_price
     *		    unit_quantity
     *		    extended_price_total
     *			
     * Description: Adds a new line item (other charge item) to the order to provide
     *              a flat percentage discount based on the order total.
     *              
     *              Assumes there is an other charge item named "ORDER DISCOUNT" defined in the system
     *              
     *              This rule will check if the "ORDER DISCOUNT" other charge item has already been added.
     *              - If it has not been added, the line item will be added with a quantity of 1 and unit
     *                  price equal to the discount(negative).
     *              - If it has been added, the unit price will be updated to the current discount(negative).
     *               
     */
    public class AddNewRowForDiscount : Rule
    {

        public override RuleResult Execute()
        {
            var result = new RuleResult();

            try
            {
                //  Get Order Line data from the data set.
                var orderLine = Data.Set.Tables["d_dw_oe_line_dataentry"];

                //  Get total order value from first line item
                var totalOrderValue = orderLine.Rows[0].Field<decimal>("extended_price_total");

                //  Get Order Header data from the data set
                var orderHeader = Data.Set.Tables["d_oe_header"];

                // Get discount percent entered by user
                var discountPct = orderHeader.Rows[0].Field<decimal?>("discount_pct") ?? 0;

                //If discount was entered by user and order total is greater than 0, set discount
                //by adding line or updating existing discount line
                if (totalOrderValue > 0 && discountPct > 0)
                {
                    DataRow discountRow;

                    //Item ID of other charge item that will be used to represent the discount
                    const string discountItemId = "ORDER DISCOUNT";
                    //Check if discount line already exists
                    var findRows = orderLine.Select("item_id = '" + discountItemId + "'");
                    if (findRows.Length > 0)
                    {
                        discountRow = findRows[0];

                        //Need to adjust order total to remove previous discount before recalculating
                        totalOrderValue = totalOrderValue + (discountRow.Field<decimal>("unit_price") * -1);
                    }
                    else
                    {
                        //Add new row
                        discountRow = Data.AddNewRow(orderLine);
                        discountRow.SetField<string>("item_id", discountItemId);
                        //Quantity of 1.0 - discount will be determined via unit price
                        discountRow.SetField<decimal>("unit_quantity", 1.0m);
                    }

                    //Calculate discount amount
                    var discountAmt = totalOrderValue * (discountPct / 100);

                    //Set unit price to discount amount (negative)
                    discountRow.SetField<decimal>("unit_price", discountAmt * -1);
                }

                result.Success = true;

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public override string GetDescription()
        {
            return "AddNewRowForDiscount";
        }

        public override string GetName()
        {
            return "AddNewRowForDiscount";
        }
    }
}
