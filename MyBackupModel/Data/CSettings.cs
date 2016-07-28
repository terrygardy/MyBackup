using System;
using MyBackupModel.Data.Models;
using System.Collections.Generic;

namespace MyBackupModel.Data
{
	public class CSettings
	{
		private static BackupDB BackupDb;
		private static Settings m_objSettings;

		public static void loadDB() { BackupDb = new BackupDB(); }

		public static void closeDB() { if (BackupDb != null) BackupDb.closeDb(); }

		public static Settings getSettings()
		{
			if ((m_objSettings == null) && (BackupDb != null)) m_objSettings = BackupDb.getSettings();

			return m_objSettings;
		}

		public static List<Folder> getFoldersTemplate() { return new List<Folder>(); }

		public static Folder generateFolder(string pv_strName, string pv_strHeader) { return new Folder() { Name = pv_strName, Header = pv_strHeader }; }

		public static void saveSettings() { if (BackupDb != null) BackupDb.saveSettings(m_objSettings); }

		public static void saveSettings(bool? pv_blnMonthBegin, bool? pv_blnMonthEnd, bool? pv_blnRepeat, bool? pv_blnDaily,
										bool? pv_blnDay, bool? pv_blnMonth, bool? pv_blnMonthly, bool? pv_blnWeek,
										bool? pv_blnWeekly, short pv_shtInterval, string pv_strTargetPath, int pv_intHour,
										int pv_intMinute, string pv_strEmail, string pv_strGoogleUser, string pv_strGooglePassword,
										List<Folder> pv_lstFolders)
		{
			if (m_objSettings == null) m_objSettings = getSettings();
			if (m_objSettings == null) m_objSettings = new Settings();

			m_objSettings.Daily = Convert.ToBoolean(pv_blnDaily);
			m_objSettings.Day = Convert.ToBoolean(pv_blnDay);
			m_objSettings.Month = Convert.ToBoolean(pv_blnMonth);
			m_objSettings.MonthBegin = Convert.ToBoolean(pv_blnMonthBegin);
			m_objSettings.MonthEnd = Convert.ToBoolean(pv_blnMonthEnd);
			m_objSettings.Monthly = Convert.ToBoolean(pv_blnMonthly);
			m_objSettings.Repeat = Convert.ToBoolean(pv_blnRepeat);
			m_objSettings.Week = Convert.ToBoolean(pv_blnWeek);
			m_objSettings.Weekly = Convert.ToBoolean(pv_blnWeekly);
			m_objSettings.Hour = pv_intHour;
			m_objSettings.Minute = pv_intMinute;
			m_objSettings.TargetPath = pv_strTargetPath;
			m_objSettings.Email = pv_strEmail;
			m_objSettings.GoogleUser = pv_strGoogleUser;
			m_objSettings.GooglePassword = pv_strGooglePassword;
			m_objSettings.AddedFolders = pv_lstFolders;

			if (((m_objSettings.Daily == true) || (m_objSettings.Monthly == true) || (m_objSettings.Weekly == true)))
			{
				m_objSettings.Interval = 0;
			}
			else
			{
				m_objSettings.Interval = pv_shtInterval;

				if (m_objSettings.Interval < 1) throw new Exception(Messages.CErrors.ErrorInterval);
			}

			saveSettings();
		}
	}
}
