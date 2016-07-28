using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBackupModel.Data.Models
{
	public class Settings
	{
		public int Id
		{
			get { return 1; }
			set { }
		}
		public bool MonthBegin { get; set; }
		public bool MonthEnd { get; set; }
		public bool Repeat { get; set; }
		public bool Daily { get; set; }
		public bool Weekly { get; set; }
		public bool Monthly { get; set; }

		public bool Day { get; set; }
		public bool Week { get; set; }
		public bool Month { get; set; }
		public short Interval { get; set; }

		public int Hour { get; set; }
		public int Minute { get; set; }

		public string TargetPath { get; set; }

		public List<Folder> AddedFolders { get; set; }
		public string Email { get; set; }
		public string GoogleUser { get; set; }
		public string GooglePassword { get; set; }

		public Settings()
		{
			MonthBegin = true;
			MonthEnd = false;
			Repeat = true;
			Daily = false;
			Weekly = false;
			Monthly = true;
			Day = false;
			Week = false;
			Month = false;
			Interval = 0;
			Hour = 0;
			Minute = 0;
			TargetPath = "";
			Email = "";
			GoogleUser = "";
			GooglePassword = "";
			AddedFolders = new List<Folder>();
		}
	}
}
