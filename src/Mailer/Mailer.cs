using System;
using System.Dynamic;
using System.Net.Mail;
using System.Text;
using Mustache = Nustache.Core;

namespace Mailer
{
    public class Mailer
    {
		/// <summary>
		///		<para>
		///			Send a templated email.
		///			Convention over configuration.
		///			Templates are Mustache http://mustache.github.com/
		///		</para>
		///		
		///		Example:
		///		<code>
		///			Mailer.Send("jim@example.com", @"password\update", new {
		///				firstname = "Jim",
		///				passwordtoken = "askf238fuhawf2983ghf"
		///			});
		///		</code>
		/// </summary>
		/// <param name="to">
		///     Email address of recipient.
		/// </param>
		/// <param name="template">
		///		<para>
		///			"Namespace" (file basename) of template from template root directory.
		///		</para>
		///		<para>
		///			There are 3 config files. Only the *.cfg file is required.
		///			<para>If a *.html mustache file is present, a HTML email will be sent.</para>
		///			<para>If a *.txt mustache file is present, a TEXT email will be sent.</para>
		///			<para>If both a *.html and *.txt file are present, a mulitpart email will be sent.</para>
		///			<para>The *.cfg file has two required fields, "from" and "subject". Both will be parsed as mustache templates.</para>
		///		</para>
		///     For example:
		///         Template root => D:\mytemplates\
		///         Template files => D:\mytemplates\foo\bar\newsletter.cfg
		///                        => D:\mytemplates\foo\bar\newsletter.html
		///                        => D:\mytemplates\foo\bar\newsletter.txt
		///         The namespace for the "newsletter" email would be
		///             => "foo\bar\newsletter"
		/// </param>
		/// <param name="templateData">
		///		Data to populate templates.
		/// </param>
		/// <param name="attachments">
		///		Optional. Array of email attachments.
		/// </param>
		public static void Send(string to, string template, dynamic templateData, params Attachment[] attachments)
		{
			var tmpl = Template.Get(template);

			if (tmpl == null)
				throw new Exception("Email template file(s) not found for " + template);
			if (tmpl.config.from == null)    
				throw new Exception("Email CFG file missing field \"from\". Template: " + template);
			if (tmpl.config.subject == null) 
				throw new Exception("Email CFG file missing field \"subject\". Template: " + template);

			dynamic data = new ExpandoObject();
			data.to          = to;
			data.from        = Mustache.Render.StringToString(tmpl.config.from, templateData);
			data.subject     = Mustache.Render.StringToString(tmpl.config.subject, templateData);
			data.attachments = attachments;
			data.html        = tmpl.html != null
				? Mustache.Render.StringToString(tmpl.html, templateData)
				: null;
			data.text        = tmpl.text != null
				? Mustache.Render.StringToString(tmpl.text, templateData)
				: null;

			var mm = createMailMessage(data);
			smtpSend(mm);
		}

		/// <summary>
		///		Send an email.
		/// </summary>
		/// <param name="to"></param>
		/// <param name="from"></param>
		/// <param name="subject"></param>
		/// <param name="body"></param>
		/// <param name="attachments">
		///		Optional. Array of email attachments.
		/// </param>
		public static void Send(string to, string from, string subject, string body, params Attachment[] attachments)
		{
			dynamic data = new ExpandoObject();
			data.to          = to;
			data.from        = from;
			data.subject     = subject;
			data.text        = body;
			data.html        = null;
			data.attachments = attachments;

			var mm = createMailMessage(data);
			smtpSend(mm);
		}

		/// <summary>
		/// Send the email via SMTP
		/// </summary>
		/// <param name="mm"></param>
		private static void smtpSend(MailMessage mm)
		{
			try
			{
				new SmtpClient(Config.SMTP_HOST).Send(mm);
				//TODO: Log Email Sent
			}
			catch (Exception)
			{
				//TODO: Log Email Error
			}
		}

		/// <summary>
		///		Creates and adds properties to a MailMessage object
		/// </summary>
		/// <param name="data">
		///		to:          String
		///		from:        String
		///		subject:     String
		///		html:        String
		///		text:        String
		///		attachments: System.Net.Mail.Attachment[]
		/// </param>
		/// <returns></returns>
		private static MailMessage createMailMessage(dynamic data)
		{
			var mm = new MailMessage(data.from, data.to);
			mm.Subject    = data.subject;
			mm.IsBodyHtml = ! string.IsNullOrWhiteSpace(data.html);
			mm.BodyEncoding = Encoding.UTF8;

			if      (! string.IsNullOrWhiteSpace(data.html)) mm.Body = data.html;
			else if (! string.IsNullOrWhiteSpace(data.text)) mm.Body = data.text;
			else                                             mm.Body = string.Empty;

			// Multipart (html & txt)
			if (!string.IsNullOrWhiteSpace(data.html) && !string.IsNullOrWhiteSpace(data.text))
			{
				mm.AlternateViews.Add(
					AlternateView.CreateAlternateViewFromString(data.text, Encoding.UTF8, "text/plain")
				);
			}

			if (data.attachments != null)
			{
				foreach (var attachment in data.attachments)
				{
					mm.Attachments.Add(attachment);
				}
			}
			
			return mm;
		}

    }
}
