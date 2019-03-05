using P21.Extensions.BusinessRule;
using System;
using System.Collections.Generic;
using System.Globalization;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.Validator
{
    /*
     * Trigger: Order Line (Items Tab) unit_quantity
     * Rule Type: Validator
     * 
     * Fields passed to the rule:
     *		d_dw_oe_line_dataentry
     *			allocated_qty
     *			disposition
     *			extended_price
     *			oe_order_item_id (alias = item_id)
     *			unit_price
     *			unit_quantity
     *			
     *		NOTE: For this single row rule to work, allocated_qty had to be added visually to the items tab
     *			  via Field Chooser (d_dw_oe_line_dataentry). Updates will not occur on the correct row
     *			  if the values are selected from d_oe_line_extended_info. This work around is NOT needed
     *			  for multi-row rules.
     *			
     * Description: Checks order line's extended price.
     *				If >= $1000 then,
     *					set the line's allocated_qty to 0 and disposition to H
     *				If < $1000 then,
     *					set the line's allocated_qty to match the unit_quantity
     *					
     *				Make sure to update the allocated_qty first, since disposition will not be editable 
     *				until after the allocated_qty is changed. Single row rules can dictate the order
     *				that P21 will update values by passing a list of column names to Data.SetFieldUpdateOrder.
     *				
     *				Also request that focus be set to the unit_price field after the rule is executed.
     */
    public class OrderLineCreditCheck : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            try
            {
                // Use the session variable to access data regarding this call to the rule including
                // MultiRow, UserID, Version, Server, Database, Language
                if (!RuleState.MultiRow)
                {
                    //  List to control update order
                    var updateSequence = new List<string>();

                    decimal unitQuantity;

                    //  Get total order line extended price
                    decimal totalPrice = Convert.ToDecimal(Data.Fields["extended_price"].FieldValue);

                    //  If price >= 1000 deallocate all lines.
                    if (totalPrice >= 1000)
                    {
                        unitQuantity = Convert.IsDBNull(Data.Fields["unit_quantity"].FieldValue)
                            ? Convert.ToDecimal(0.0)
                            : Convert.ToDecimal(Data.Fields["unit_quantity"].FieldValue);

                        if (unitQuantity > 0)
                        {
                            //Make sure to update the allocated_qty first, since disposition will not be editable
                            //until after the allocated_qty is changed.
                            Data.Fields["allocated_qty"].FieldValue = "0.0";
                            updateSequence.Add("allocated_qty");
                            Data.Fields["disposition"].FieldValue = "H";
                            updateSequence.Add("disposition");
                        }
                    }
                    else
                    {
                        unitQuantity = Convert.IsDBNull(Data.Fields["unit_quantity"].FieldValue)
                            ? Convert.ToDecimal(0.0)
                            : Convert.ToDecimal(Data.Fields["unit_quantity"].FieldValue);

                        var allocatedQuantity = Convert.IsDBNull(Data.Fields["allocated_qty"].FieldValue)
                            ? Convert.ToDecimal(0.0)
                            : Convert.ToDecimal(Data.Fields["allocated_qty"].FieldValue);

                        if (!unitQuantity.Equals(allocatedQuantity))
                        {
                            Data.Fields["allocated_qty"].FieldValue = Convert.ToString(unitQuantity, CultureInfo.InvariantCulture);
                            updateSequence.Add("allocated_qty");
                        }
                    }

                    //Request focus be set on unit_price field instead of UOM, which normally follows unit_quantity
                    Data.SetFocus("unit_price");

                    //Request P21 to update fields in the same order that the code in this rule updated them
                    Data.SetFieldUpdateOrder(updateSequence);

                    result.Success = true;
                }
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
            return "Order Line Credit Check";
        }

        public override string GetName()
        {
            return "OrderLineCreditCheck";
        }
    }
}
