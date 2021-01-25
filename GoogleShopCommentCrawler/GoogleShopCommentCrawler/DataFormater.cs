using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GoogleShopCommentCrawler
{
	public interface IDataFormater
	{
		List<TResult> Format<TResult>(List<string> ReponseBodyList);
	}

	public class DataFormater : IDataFormater
	{
		IDataExtractor _dataExtractor;

		public DataFormater(IDataExtractor DataExtractor)
		{
			_dataExtractor = DataExtractor;
		}

		public virtual List<TResult> Format<TResult>(List<string> ResponseBodyList)
		{
			List<TResult> DataList = new List<TResult>();
			foreach (string ResponseBody in ResponseBodyList)
			{
				//從 ResponseBody 中取得 CommentBody 
				string CommentBody = GetCommentBody(ResponseBody);
				//從 CommentBody 中取得全部的 Comment
				List<string> CommentList = GetCommentList(CommentBody);
				//把 CommentList 全部轉換成指定的資料格式 TResult
				List<TResult> ResultData = GetData<TResult>(CommentList);
				//從 ResponseBody 取得一些只能從ResponseBody取得的資料同時寫入 ResultData
				_dataExtractor.ExtractFromResponseBody(ResultData, ResponseBody);
				DataList.AddRange(ResultData);
			}
			return DataList;
		}
		protected virtual List<TResult> GetData<TResult>(List<string> CommentList)
		{
			List<TResult> ResultList = new List<TResult>();
			foreach (string Comment in CommentList)
			{
				//多筆 Comment 一個一個轉換成 TResult
				ResultList.Add(_dataExtractor.ExtractFromComment<TResult>(Comment));
			}
			return ResultList;
		}

		protected virtual string GetCommentBody(string ResponseBody)
		{
			var StartPatten = @"<div data-google-review-count=""[\d]+"" data-next-page-start-index=""[\d]+"" data-next-page-token=""[\w]+"" class=""gws-localreviews__general-reviews-block"">";
			var EndPatten = @"4;\[9\]";
			Regex reg = new Regex(StartPatten);
			var Startmatch = reg.Match(ResponseBody);
			reg = new Regex(EndPatten);
			var Endmatch = reg.Match(ResponseBody);
			return ResponseBody.Substring(Startmatch.Index, Endmatch.Index - Startmatch.Index);
		}

		protected virtual List<string> GetCommentList(string CommentBody)
		{
			List<string> resultList = new List<string>();
			string StartPatten = @"<div jscontroller=""e6Mltc""";
			string EndPatten = @"</span>(</div>)+</g-snackbar></button>(</div>)+";
			Regex reg = new Regex(StartPatten);
			var StartMatches = reg.Matches(CommentBody);
			foreach (Match match in StartMatches)
			{
				Regex reg1 = new Regex(EndPatten);
				Match EndMatch = reg1.Match(CommentBody, match.Index);
				var CommentString = string.Empty;
				if (EndMatch.Success)
					CommentString = CommentBody.Substring(match.Index, EndMatch.Index - match.Index + EndMatch.Length);
				else
				{
					//找下一個起點當這次的終點
					var EndIndex = CommentBody.IndexOf(StartPatten, match.Index + match.Length);
					//找到則用這個起點當這次終點 找不到則使用CommentBody的最後一個字當終點
					if (EndIndex > 0)
						CommentString = CommentBody.Substring(match.Index, EndIndex - match.Index);
					else
						CommentString = CommentBody.Substring(match.Index, CommentBody.Length - match.Index);
				}

				resultList.Add(CommentString);
			}
			return resultList;
		}
	}

}
