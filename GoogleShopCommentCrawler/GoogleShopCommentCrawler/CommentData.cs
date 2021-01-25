using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleShopCommentCrawler
{
	public class CommentData
	{
		public string Shop { get; set; }
		public string Author { get; set; }
		public string Time { get; set; }
		public string Comment { get; set; }
		public double? Stars { get; set; }
	}
}
