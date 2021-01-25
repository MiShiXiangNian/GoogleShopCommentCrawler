using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleShopCommentCrawler
{
	public static class Extensions
	{
		public static void AddGoogleShopCommentCrawler(this IServiceCollection services)
		{
			services.AddScoped<Crawler>();
			services.AddScoped<IHttpCaller, HttpCaller>();
			services.AddScoped<IDataExtractor, DataExtractor>();
			services.AddScoped<IDataFormater, DataFormater>();
		}

		public static string ExtractString(string StartPatten, string EndPatten, string Tartget)
		{
			string Result = null;
			int StartIndex = Tartget.IndexOf(StartPatten);
			if (StartIndex < 0)
				return Result;
			int EndIndex = Tartget.IndexOf(EndPatten, StartIndex + StartPatten.Length);
			if (EndIndex < 0)
				return Result;
			Result = Tartget.Substring(StartIndex + StartPatten.Length, EndIndex - (StartIndex + StartPatten.Length));
			return Result == string.Empty ? null : Result;
		}
	}
}
