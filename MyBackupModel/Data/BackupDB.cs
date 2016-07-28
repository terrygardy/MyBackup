using System;
using System.IO;
using System.Linq;
using LiteDB;
using MyBackupModel.Data.Models;

namespace MyBackupModel.Data
{
	public class BackupDB
	{
		private LiteDatabase m_dbDbModel;

		public BackupDB()
		{
			string strDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyBackup");

			if ((!string.IsNullOrEmpty(strDbPath)) && (!Directory.Exists(strDbPath))) Directory.CreateDirectory(strDbPath);

			strDbPath = Path.Combine(strDbPath, "BackupSettings.db");

			m_dbDbModel = new LiteDatabase(strDbPath);
		}

		private LiteCollection<Settings> getSettingsCollection() { return m_dbDbModel.GetCollection<Settings>("MyBackup"); }

		public Settings getSettings()
		{
			var lclSettings = getSettingsCollection();
			Settings objSettings = lclSettings.FindAll().FirstOrDefault();

			if (objSettings == null)
			{
				objSettings = new Settings();
				lclSettings.Insert(objSettings);
			}

			return objSettings;
		}

		public void saveSettings(Settings pv_objSettings)
		{
			var lclSettings = getSettingsCollection();

			lclSettings.Update(pv_objSettings);
		}

		public void closeDb() { if (m_dbDbModel != null) m_dbDbModel.Dispose(); m_dbDbModel = null; }
	}
}
