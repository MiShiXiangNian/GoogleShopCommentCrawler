using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GoogleShopCommentCrawler;

namespace GoogleShopCommentCrawler
{
	public interface IDataExtractor
	{
		TResult ExtractFromComment<TResult>(string Comment);

		void ExtractFromResponseBody<TData>(List<TData> Data, string ResponseBody);
	}

	public class DataExtractor : IDataExtractor
	{
		public virtual TResult ExtractFromComment<TResult>(string Comment)
		{
			Type ResultType = typeof(TResult);
			object ResultObject = Activator.CreateInstance(ResultType);
			foreach (var prop in ResultType.GetProperties())
			{
				switch (prop.Name)
				{
					case "Author":
						prop.SetValue(ResultObject, GetAuthor(Comment));
						break;
					case "Time":
						prop.SetValue(ResultObject, GetTime(Comment));
						break;
					case "Stars":
						prop.SetValue(ResultObject, GetStar(Comment));
						break;
					case "Comment":
						prop.SetValue(ResultObject, GetComment(Comment));
						break;
				}
			}

			return (TResult)ResultObject;
		}

		public virtual void ExtractFromResponseBody<TData>(List<TData> DataList, string ResponseBody)
		{
			if (DataList.Any())
			{
				Type DataType = typeof(TData);
				foreach (var prop in DataType.GetProperties())
				{
					switch (prop.Name)
					{
						case "Shop":
							DataList.ForEach(Data => DataType.GetProperty("Shop").SetValue(Data, GetShop(ResponseBody)));
							break;
					}
				}
			}
		}

		#region GetShop

		protected virtual string GetShop(string ResponseBody)
		{
			string StartPatten = @"<div class=""P5Bobd"">";
			string EndPatten = @"</div>";
			return Extensions.ExtractString(StartPatten, EndPatten, ResponseBody);
		}

		#endregion

		#region GetAuthor

		protected virtual string GetAuthor(string Comment)
		{
			var DivClassTSUbDbTag = GetDivClassTSUbDbTag(Comment);
			var ATag = GetATag(DivClassTSUbDbTag);
			Regex reg = new Regex(@">[\s\S]+<");
			var match = reg.Match(ATag);
			var AuthorName = match.Value.Substring(1, match.Length - 2);
			return AuthorName;
		}

		string GetDivClassTSUbDbTag(string Comment)
		{
			string StartPatten = @"<div class=""TSUbDb"">";
			string EndPatten = @"</div>";
			return Extensions.ExtractString(StartPatten, EndPatten, Comment);
		}

		string GetATag(string DivTagString)
		{
			Regex reg = new Regex(@"<a[\s\S]+</a>");
			var match = reg.Match(DivTagString);
			return DivTagString.Substring(match.Index, match.Length);
		}

		#endregion

		#region GetComment 

		protected virtual string GetComment(string Comment)
		{
			var result = string.Empty;
			if (Comment.IndexOf("review-snippet") > 0)
				result = SnippetComment(Comment);
			else
			{
				result = NoSnippetComment(Comment);
			}
			return result;
		}

		string SnippetComment(string Comment)
		{
			string StartPatten = @"<span class=""review-full-text hide-focus-ring"" tabindex=""-1"" style=""display:none"">";
			string EndPatten = @"</span>";
			return Extensions.ExtractString(StartPatten, EndPatten, Comment);
		}

		string NoSnippetComment(string Comment)
		{
			string StartPatten = @"<span jscontroller=""P7L8k"" jsaction=""rcuQ6b:npT2md"">";
			string EndPatten = @"</span>";
			return Extensions.ExtractString(StartPatten, EndPatten, Comment);
		}

		#endregion

		#region GetStar

		protected virtual double? GetStar(string Comment)
		{
			double? Result = null;
			var StarSpan = GetStarSpan(Comment);
			if (!string.IsNullOrWhiteSpace(StarSpan))
			{
				Regex reg = new Regex(@"[\d].[\d]");
				var match = reg.Match(StarSpan);
				if (match.Success)
					Result = Convert.ToDouble(match.Value);
			}
			return Result;
		}

		string GetStarSpan(string Comment)
		{
			string Result = null;
			Regex reg = new Regex(@"<span class=""Fam1ne EBe2gf"" aria-label=""評等：[\d].[\d] \(最高：5\)，"" role=""img""><span style=""width:[\d]+px""></span></span>");
			var match = reg.Match(Comment);
			if (match.Success)
			{
				Result = Comment.Substring(match.Index, match.Length);
			}
			return Result;
		}

		#endregion

		#region GetTime

		protected virtual string GetTime(string Comment)
		{
			string StartPatten = @"<span class=""dehysf lTi8oc"">";
			string EndPatten = @"</span>";
			return Extensions.ExtractString(StartPatten, EndPatten, Comment);
		}

		#endregion
	}
}
