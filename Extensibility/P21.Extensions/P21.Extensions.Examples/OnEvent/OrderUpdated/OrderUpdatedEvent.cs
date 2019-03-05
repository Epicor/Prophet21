using P21.Extensions.BusinessRule;
using System;
using System.IO;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.OnEvent.OrderUpdated
{
    /*
     * Trigger: Order Updated Event
     * Rule Type: OnEvent
     * 
     * Fields passed to the rule:
     *		OrderHeader
     *		    key_value - order number
     *		    action - possible values are ADD & UPDATE
     *			
     * Description: This is a very simple rule to show how the Order Updated event based rule can be implemented.
     *              When an OnEvent type rule is set up in P21 for the OrderUpdated event, this rule will be fired.
     *              The rule simply drops a text file in the C:\temp directory containing the order number that
     *              was updated and the action (ADD if the order was just created, UPDATE if the order was modified).
     *              
     *              This event type could be used for any workflow processing that is needed after an order is
     *              saved to the database. For any of the <xxx>Updated business rule events in P21 (OrderUpdated,
     *              InvoiceUpdated, etc.), you'll notice that the data exposed to the rule will be key_value and 
     *              action. Since the information that has been updated has already been committed to the P21 
     *              database, any additional information can be gathered by querying the database.
     */

    public class OrderUpdatedEvent : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult();

            try
            {
                string[] lines = {
                    $"Order: {Convert.ToString(Data.Set.Tables["OrderHeader"].Rows[0]["key_value"])}",
                    "Action: " + Convert.ToString(Data.Set.Tables["OrderHeader"].Rows[0]["action"])};

                File.WriteAllLines(@"C:\temp\OrderUpdatedEventRule.txt", lines);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public override string GetDescription()
        {
            return "Order Updated Event";
        }

        public override string GetName()
        {
            return "OrderUpdatedEvent";
        }
    }
}

