﻿using System;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;

#pragma warning disable 1591

namespace Elmah.Contrib.WebApi
{
	public class ElmahExceptionLogger : ExceptionLogger
	{
		public override void Log(ExceptionLoggerContext context)
		{
			// Retrieve the current HttpContext instance for this request.
			HttpContext httpContext = GetHttpContext(context.Request);

			if (httpContext == null)
				return;

            // Wrap the exception in an HttpUnhandledException so that ELMAH can capture the original error page.
            string errorMessage = context.Exception.Message;
            try
            {
                string data = context.Request.Content.ReadAsStringAsync().Result;
                errorMessage += " " + data;
            }
            catch { }
            Exception exceptionToRaise = new HttpUnhandledException(errorMessage, context.Exception);

            // Send the exception to ELMAH (for logging, mailing, filtering, etc.).
            ErrorSignal signal = ErrorSignal.FromContext(httpContext);
			signal.Raise(exceptionToRaise, httpContext);
		}

		private static HttpContext GetHttpContext(HttpRequestMessage request)
		{
			if (request == null)
				return null;

			object value;
			if (request.Properties.TryGetValue("MS_HttpContext", out value))
			{
				HttpContextBase context = value as HttpContextBase;
				if (context != null)
					return context.ApplicationInstance.Context;
			}

			return HttpContext.Current;
		}
	}
}

#pragma warning restore 1591
