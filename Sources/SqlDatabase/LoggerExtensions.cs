﻿using System;
using System.Text;

namespace SqlDatabase
{
    internal static class LoggerExtensions
    {
        public static void Error(this ILogger logger, string message, Exception error)
        {
            var text = new StringBuilder();
            if (!string.IsNullOrEmpty(message))
            {
                text.Append(message);
            }

            var ex = error;
            while (ex != null)
            {
                if (text.Length > 0)
                {
                    text.Append(" ---> ");
                }

                text.Append(ex.Message);
                ex = ex.InnerException;
            }

            logger.Error(text.ToString());
            logger.Info(error.StackTrace);
        }

        public static void Error(this ILogger logger, Exception error) => Error(logger, null, error);
    }
}
