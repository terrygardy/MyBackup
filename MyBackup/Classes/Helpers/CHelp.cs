using System;

namespace MyBackupSettings.Classes.Helpers
{
	public class CHelp
	{
		public static CTimeStamp getTimeStamp(string pv_strTimeString)
		{
			var arrTime = pv_strTimeString.Trim().Split(@":".ToCharArray());
			var objTimeStamp = new CTimeStamp() { Hour = -1, Minute = -1 };

			if ((arrTime.GetLength(0) == 2) && (arrTime[0].Length == 2) && (arrTime[1].Length == 2))
			{
				try
				{
					objTimeStamp.Hour = Convert.ToInt32(arrTime[0]);
					objTimeStamp.Minute = Convert.ToInt32(arrTime[1]);
				}
				catch (Exception) { objTimeStamp.Hour = -1; objTimeStamp.Minute = -1; }
			}

			return objTimeStamp;
		}
	}

	public class CTimeStamp
	{
		public int Hour { get; set; }
		public int Minute { get; set; }
	}
}
