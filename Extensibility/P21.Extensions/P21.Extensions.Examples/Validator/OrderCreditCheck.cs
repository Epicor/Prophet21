using P21.Extensions.BusinessRule;
using System;
using System.Data;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.Validator
{
    /*
     * Trigger: Order Line (Items Tab) unit_quantity
     * Rule Type: Validator
     * 
     * Fields passed to the rule:
     *		d_dw_oe_line_dataentry
     *			extended_price
     *			oe_order_item_id (alias = item_id)
     *			unit_price
     *		d_oe_line_extended_info
     *			allocated_qty
     *			disposition
     *			quantity_available
     *			unit_quantity
     *			
     * Description: Checks order total by summing all line's extended prices.
     *				If >= $1000 then,
     *					set all lines' allocated_qty to 0 and disposition to H
     *				If < $1000 then,
     *					set all lines' allocated_qty to match the unit_quantity
     *					
     *				Make sure to update the allocated_qty first, since disposition will not be editable 
     *				until after the allocated_qty is changed. Multi-row rules set Data.UpdateByOrderCoded
     *				to true by default so unless that is changed they will update values in P21 in the
     *				same order that they are updated in the rule code.
     *				
     *				Also request that focus be set to the unit_price field after the rule is executed.
     */
    public class OrderCreditCheck : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            try
            {
                // Use the session variable to access data regarding this call to the rule including
                // MultiRow, UserID, Version, Server, Database, Language
                if (RuleState.MultiRow)
                {

                    //  Get Order Line data from the data set.
                    var orderLine = Data.Set.Tables["d_dw_oe_line_dataentry"];
                    //  Get total order line extended price
                    var totalPrice = Convert.ToDecimal(Convert.ToString(orderLine.Compute("Sum(extended_price)", "")));

                    //  If price >= 1000 deallocate all lines.

                    //  Get Order Line Extended Info data from the data set
                    var orderLineExtInfo = Data.Set.Tables["d_oe_line_extended_info"];
                    if (totalPrice >= 1000)
                    {
                        foreach (DataRow row in orderLineExtInfo.Rows)
                        {
                            if (row.Field<decimal>("unit_quantity") <= 0)
                                continue;

                            //Make sure to update the allocated_qty first, since disposition will not be editable
                            //until after the allocated_qty is changed.
                            row.SetField<decimal>("allocated_qty", 0.0m);
                            row.SetField<string>("disposition", "H");
                        }
                    }
                    else
                    {
                        foreach (DataRow row in orderLineExtInfo.Rows)
                        {
                            var unitQuantity = row.Field<decimal>("unit_quantity");
                            var allocatedQuantity = row.Field<decimal>("allocated_qty");
                            var availableQuantity = row.Field<decimal>("quantity_available");

                            //Don't try to allocate more than the available quantity.
                            if (unitQuantity > availableQuantity + allocatedQuantity)
                            {
                                unitQuantity = (availableQuantity + allocatedQuantity);

                                //In this case the line will need a disposition - we're setting it back to B
                                row.SetField<string>("disposition", "B");
                            }

                            if (!unitQuantity.Equals(allocatedQuantity))
                            {
                                row.SetField<decimal>("allocated_qty", unitQuantity);
                            }
                        }
                    }

                    //Request focus be set on unit_price field instead of UOM, which normally follows unit_quantity
                    Data.SetFocus("unit_price", Data.TriggerRow);

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
            return "Order Credit Check";
        }

        public override string GetName()
        {
            return "OrderCreditCheck";
        }
    }
}
