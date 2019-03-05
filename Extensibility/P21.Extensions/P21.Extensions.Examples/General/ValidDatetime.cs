using P21.Extensions.BusinessRule;
using System;
using System.Globalization;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.General
{
    /*
     * Description: This is a single row rule that will validate that each field passed in has a proper date format.
     */
    public class ValidDatetime : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            string[] formats = { "mm/dd/yyyy",
                                 "m/d/yyyy",
                                 "m/dd/yyyy",
                                 "mm/d/yyyy",
                                 "mm-dd-yyyy",
                                 "m-d-yyyy",
                                 "m-dd-yyyy",
                                 "mm-d-yyyy" };


            foreach (DataField field in Data.Fields)
            {
                if (field.ClassName == "global")
                    continue;

                if (DateTime.TryParseExact(field.FieldValue, formats, new CultureInfo("en-US"),
                    DateTimeStyles.AssumeLocal, out _))
                    continue;

                result.Message = $"Incorrectly formatted date '{field.FieldValue}'";
                result.Success = false;
            }

            return result;
        }

        public override string GetDescription()
        {
            return "Validates the field contains a valid date representation.";
        }

        public override string GetName()
        {
            return "Valid Datetime";
        }
    }
}