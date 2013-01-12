using System.Configuration;

namespace Mailer
{
	internal class Config
	{
		internal static string TEMPLATES_ROOT
		{
			get { return ConfigurationManager.AppSettings["EMAIL_TEMPLATES_ROOT"]; }
		}

		internal static string SMTP_HOST
		{
			get { return ConfigurationManager.AppSettings["SMTP_HOST"]; }
		}
	}
}
