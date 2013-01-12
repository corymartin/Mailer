using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Mailer
{
	internal class Template
	{
		/// <summary>
		///		Given a template's path, returns the contents and type.
		///		Templates are Mustache http://mustache.github.com/
		/// </summary>
		/// <param name="template">
		///		Template namespace from root (set in config).
		///		AKA the templates path but without a file extension.
		///		E.g. newsletter\weekly\update
		///		or   newsletter/weekly/update
		///	</param>
		///	<returns>
		///		Returns an object of template data and contents:
		///		
		///		html:   {String} Contents of HTML template file, or null if there isn't one.
		///		text:   {String} Contents of TXT template file, or null if there isn't one.
		///		config: {Dictionary[string,string]} Key/Value pairs from CFG file.
		///		
		///		If there is no CFG file, null is returned.
		/// </returns>
		internal static dynamic Get(string template)
		{
			template = template.Replace('/', Path.DirectorySeparatorChar);
			var key = template.TrimStart(
				'/',
				Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar, 
				Path.PathSeparator, Path.VolumeSeparatorChar
			).Trim().ToLower();

			#if ! DEBUG
			if (memoizationCache_Get.ContainsKey(key)) return memoizationCache_Get[key];
			#endif

			var htmlfile = Path.Combine(Config.TEMPLATES_ROOT, template + ".html");
			var txtfile  = Path.Combine(Config.TEMPLATES_ROOT, template + ".txt");
			var cfgfile  = Path.Combine(Config.TEMPLATES_ROOT, template + ".cfg");

			// CFG file is req'd
			if (! File.Exists(cfgfile)) return null;

			var htmlExists = File.Exists(htmlfile);
			var txtExists  = File.Exists(txtfile);

			return memoizationCache_Get[key] = new {
				html   = htmlExists ? File.ReadAllText(htmlfile) : string.Empty,
				text   = txtExists ? File.ReadAllText(txtfile) : string.Empty,
				config = parseConfig(cfgfile)
			};
		}

		/// <summary>
		///		Simple cache for template info.
		///		Key is template namespace.
		/// </summary>
		private static Dictionary<string, dynamic> memoizationCache_Get = 
			new Dictionary<string, dynamic>();

		/// <summary>
		///		Parses the a *.cfg file and returns the key/value pairs.
		/// </summary>
		/// <param name="cfgfile">
		///		Path to *.cfg file
		///	</param>
		/// <returns>
		///		Key/Value pairs from a CFG file.
		/// </returns>
		private static dynamic parseConfig(string cfgfile)
		{
			var lines = File.ReadLines(cfgfile, System.Text.Encoding.UTF8);
			lines = lines.Where(line => ! String.IsNullOrWhiteSpace(line));
			dynamic cfg = new ExpandoObject();
			foreach (var line in lines)
			{
				var idx = line.IndexOf(':');
				if (idx == -1) continue;
				var key = line.Substring(0, idx).Trim();
				var val = line.Substring(idx + 1).Trim();
				((IDictionary<string,dynamic>)cfg)[key] = val;
			}
			return cfg;
		}

	}
}
