using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class ExceptionExtensions
    {
        public static string GetAllMessage(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            _GetMessage(sb, ex);
            return sb.ToString();
        }

        static void _GetMessage(StringBuilder sb, Exception ex)
        {
            if (sb.Length > 0)
            {
                sb.Append("--->");
            }
            sb.Append(ex.Message);
            if (ex.InnerException != null)
            {
                _GetMessage(sb, ex.InnerException);
            }
        }
    }
}
