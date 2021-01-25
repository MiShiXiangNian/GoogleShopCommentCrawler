using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace GoogleShopCommentCrawler
{
	public interface IHttpCaller
	{
		List<string> GetAllResponseBody(string Url);
	}

	public class HttpCaller : IHttpCaller
	{
		public virtual List<string> GetAllResponseBody(string Url)
		{
			List<string> ResponseBodyList = new List<string>();
			int StartIndex = 0;
			string NextPageToken = string.Empty;
			//把Url的 start_index:XXXX 與 next_page_token:XXXX 變成 start_index:{0} 與 next_page_token:{1} 方便後面使用
			string UrlFormat = GetUrlFormat(Url);

			while (NextPageToken != null)
			{
				string UsedUrl = string.Format(UrlFormat, StartIndex, NextPageToken);
				string ResponseBody = GetResponseBody(UsedUrl);
				ResponseBodyList.Add(ResponseBody);
				//從ResponseBody取得下一個 next_page_token 的值
				NextPageToken = GetNextPageTokenFromResponseBody(ResponseBody);
				StartIndex += 10;
			}
			return ResponseBodyList;
		}

		protected virtual string GetResponseBody(string Url)
		{
			var result = string.Empty;
			using (HttpClient client = new HttpClient())
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-tw", 0.9));
				var response = client.GetAsync(Url).Result;
				response.EnsureSuccessStatusCode();
				result = response.Content.ReadAsStringAsync().Result;
			}
			return result;
		}

		protected virtual string GetNextPageTokenFromResponseBody(string ResponseBody)
		{
			string StartPatten = @"data-next-page-token=""";
			string EndPatten = @"""";
			return Extensions.ExtractString(StartPatten, EndPatten, ResponseBody);
		}

		protected virtual string GetUrlFormat(string Url)
		{
			if (!IsVaildUrl(Url))
				throw new Exception("Url is not Vaild.");
			return Url.Replace(GetNextPageToken(Url), "start_index:{0},")
				.Replace(GetStartIndex(Url), "next_page_token:{1},");
		}

		protected virtual bool IsVaildUrl(string Url)
		{
			string Patten = @"https://www.google.com/async/reviewDialog\?[\s\S]*&yv=[\s\S]+&async=feature_id:[\s\S]*,review_source:[\s\S]*,sort_by:[\s\S]*,start_index:[\s\S]*,is_owner:[\s\S]*,filter_text:[\s\S]*,associated_topic:[\s\S]*,next_page_token:[\s\S]*,async_id_prefix:[\s\S]*,_pms:[\s\S]*,_fmt:[\s\S]*";
			Regex reg = new Regex(Patten);
			return reg.IsMatch(Url);
		}

		string GetNextPageToken(string Url)
		{
			string StartIndexPatten = @"start_index:[\d]+,";
			Regex reg = new Regex(StartIndexPatten);
			Match match = reg.Match(Url);
			if (match.Success)
				return match.Value;
			else
				return null;
		}

		string GetStartIndex(string Url)
		{
			string NextPageTokenPatten = @"next_page_token:[\s\S]+,";
			Regex reg = new Regex(NextPageTokenPatten);
			Match match = reg.Match(Url);
			if (match.Success)
				return match.Value;
			else
				return null;
		}
	}
}
