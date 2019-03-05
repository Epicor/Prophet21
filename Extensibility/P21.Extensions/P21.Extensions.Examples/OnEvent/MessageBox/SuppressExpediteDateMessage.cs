using P21.Extensions.BusinessRule;
using System;
using System.Data;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.OnEvent.MessageBox
{
    /*
     * Rule Type: On Event
     * Event: Message Box Opening
     * 
     * Simple rule to show how to change the option that is being selected for a message-box
     * in P21 and to suppress that message.
     * 
     * NOTE: Because of the way P21 messages work, it is recommended to always check the message
     *       number AND the full message text to make sure your rule isn't going to unintentionally
     *       apply to more message-boxes than you're expecting.
     */
    public class SuppressExpediteDateMessage : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            try
            {
                var row = Data.Set.Tables["MessageBoxData"].Rows[0];
                var msgNo = Convert.ToString(row["message_no"]);
                var userText = Convert.ToString(row["user_text"]);

                if (msgNo == "9656")
                    if (!string.IsNullOrWhiteSpace(userText))
                    {
                        if (userText == "Would you like to update the line items to the new expedite date?")
                        {
                            // Set the value to suppress the message.                   
                            row.SetField<string>("suppress_message", "Y");

                            // IMPORTANT!!!
                            // On the P21 side, if a rule returns with suppress_message = 'Y', then the
                            // responding P21 code presumes the default_button value is the option that
                            // should be used. In this case, the message asks
                            //      "Would you like to update the line items to the new expedite date?"
                            // and the default_button value presumed by the application is 2 (No). So,
                            // in order for this rule to work properly (by suppressing the message and
                            // always updating the lines with the new expedite date value), then the rule
                            // code must ALSO reset the default_button value to 1 (Yes) so that on return
                            // to the P21 code the application will consider the answer as 'Yes'.
                            row.SetField<int>("default_button", 1);
                        }
                    }
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Message = e.Message;
            }

            return result;
        }

        public override string GetDescription()
        {
            return "Suppresses messaging prompting whether to copy the header expedite date to all lines in" +
                        " OE after the header expedite date value is changed. Defaults answer to 'Yes'";
        }

        public override string GetName()
        {
            return "SuppressOEExpediteDateMessage";
        }
    }
}
