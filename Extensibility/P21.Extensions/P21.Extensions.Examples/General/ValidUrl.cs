using P21.Extensions.BusinessRule;
using System;
using System.Text.RegularExpressions;
using Rule = P21.Extensions.BusinessRule.Rule;

namespace P21.Extensions.Examples.General
{
    /*
     * Description: This is a single row rule that will validate that each field passed in has a proper URL format.
     */
    public class ValidUrl : Rule
    {
        public override RuleResult Execute()
        {
            var result = new RuleResult { Success = true };

            foreach (DataField field in Data.Fields)
            {
                if (field.ClassName == "global")
                    continue;

                if (Uri.TryCreate(field.FieldValue, UriKind.Absolute, out var uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ||
                        uri.Scheme == Uri.UriSchemeFile)
                        continue;

                    result.Message =
                        $"Incorrect URL scheme ({uri.Scheme}) for field {field.TableName}.{field.FieldName}";

                    result.Success = false;
                }
                else if (IsAlternativeUrlValid(field.FieldValue))
                {
                    result.Success = true;
                }
                else
                {
                    // Let the user know that the call failed.
                    result.Message = $"Incorrect URL format for field {field.TableName}.{field.FieldName}";
                    result.Success = false;
                }
            }

            return result;
        }

        public override string GetDescription()
        {
            return "Validates URL field has correct format";
        }

        public override string GetName()
        {
            return "Valid URL";
        }

        private static bool IsAlternativeUrlValid(string url)
        {
            const string expression = @"^(www.)[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$";

            return Regex.IsMatch(url, expression);
        }
    }
}
