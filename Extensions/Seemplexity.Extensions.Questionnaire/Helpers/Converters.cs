using Seemplexity.Extensions.Questionnaire.DataModel;

namespace Seemplexity.Extensions.Questionnaire.Helpers
{
    public static class Converters
    {
        public static FormatType StringToFormatType(string stringToConvert)
        {
            var result = FormatType.Undefined;
            switch (stringToConvert)
            {
                case "Text":
                    result = FormatType.Text;
                    break;
                case "Digits":
                    result = FormatType.Digits;
                    break;
                case "Date":
                    result = FormatType.Date;
                    break;
                case "Email":
                    result = FormatType.Email;
                    break;
            }
            return result;
        }
    }
}
