using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleShopCommentCrawler
{
	public class Crawler
	{
		IHttpCaller _httpCaller;
		IDataFormater _dataFormater;

		public Crawler(IHttpCaller HttpCaller, IDataFormater DataFormater)
		{
			_httpCaller = HttpCaller;
			_dataFormater = DataFormater;
		}

		public List<TResult> GetData<TResult>(string Url)
		{
			//使用多次HttpGet取得全部的ResponseBody
			List<string> AllResponseBody = _httpCaller.GetAllResponseBody(Url);
			//把ResponseBody的內容格式化成自訂的資料類型TResult
			List<TResult> Data = _dataFormater.Format<TResult>(AllResponseBody);

			return Data;
		}
	}
}
