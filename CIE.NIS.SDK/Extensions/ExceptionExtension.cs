
namespace CIE.NIS.SDK.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ExceptionExtension
    {
        public static string ToLastInnerMessage(this Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;

            return ex.InnerException.ToLastInnerMessage();
        }

        public static List<string> ToMessagesList(this Exception ex)
        {
            var last = false;
            var messages = new List<string>();
            var ex1 = ex;
            while (!last)
            {
                messages.Add(ex1.Message);
                if (ex1.InnerException == null)
                    last = true;
                else
                    ex1 = ex1.InnerException;
            }
            return messages.Count > 0 ? messages : null;
        }

        public static string ToUniqueMessage(this Exception ex)
        {
            var sb = new StringBuilder();
            foreach (var message in ex.ToMessagesList())
            {
                sb.Append(message + " ");
            }
            return sb.ToString().Trim();
        }
    }
}
