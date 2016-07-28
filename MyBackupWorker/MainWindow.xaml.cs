using System;
using System.Collections.Generic;
using System.Windows;
using MyBackupModel.Data;
using System.IO;
using System.Windows.Threading;
using System.Threading.Tasks;
using Email;

namespace MyBackupWorker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string m_strLogName = "MyBackup";
		private string m_strEmailHost = "smtp.live.com";
		private int m_intEmailPort = 25;
		private string m_strEmailUser = "terry.gardner@live.de";
		private string m_strEmailPasswort = "pointerEmail2015";
		private bool m_blnEmailSSL = true;
		private bool m_blnHTML = true;
		private List<MyBackupModel.Data.Models.Folder> m_lstFolders = null;

		private List<MyBackupModel.Data.Models.Folder> AddedFolders
		{
			get
			{
				if (m_lstFolders == null) m_lstFolders = CSettings.getSettings().AddedFolders;

				return m_lstFolders;
			}
		}

		private string TargetFolder { get; set; }
		private string Email { get; set; }
		private int CopiedCount { get; set; }
		private int CountFolders { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			CopiedCount = 0;
			CSettings.loadDB();
			Log.Log.emptyLog(m_strLogName);
			var objSettings = CSettings.getSettings();

			TargetFolder = objSettings.TargetPath;
			Email = objSettings.Email;
			m_lstFolders = objSettings.AddedFolders;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var objTask = startCopy();
		}

		private async Task<string> startCopy()
		{
			if (TargetFolder != "")
			{
				await Task.Run(new Action(() =>
				{
					if (!string.IsNullOrEmpty(Email))
					{
						try
						{
							Main.send(m_strEmailUser, Email, "MyBackup - gestartet", Main.buildEmailBody(Messages.CMessages.Salutation,
										Messages.CMessages.BackupBegun + ".", Messages.CMessages.Valediction), m_blnHTML,
										m_strEmailHost, m_intEmailPort, m_strEmailUser, m_strEmailPasswort, m_blnEmailSSL);
						}
						catch (Exception ex) { Log.Log.extendLog(m_strLogName, Messages.CMessages.EmailNotSent + ": " + ex.Message); }
					}

					while (!Directory.Exists(TargetFolder))
					{
						lblCurrent.Dispatcher.Invoke(() =>
						{
							lblFile.Text = Messages.CMessages.TargetFolderNotFound + ": ";
							lblCurrent.Text = TargetFolder;
						});

						System.Threading.Thread.Sleep(3000);
					}

					CountFolders = 0;

					lblCurrent.Dispatcher.Invoke(() =>
					{
						lblFile.Text = Messages.CMessages.SelectedFoldersSearched + "...";
						lblCurrent.Text = "";
					});

					foreach (var objFolder in AddedFolders)
					{
						countChildFolders(objFolder.Name);
					}

					lblCurrent.Dispatcher.Invoke(() =>
					{
						lblFile.Text = Messages.CMessages.FilesBeingBackedUp + ": ";
						lblCurrent.Text = "...";
					});

					foreach (var objFolder in AddedFolders)
					{
						moveContent(objFolder.Name);

						updatePercent();
					}

					CopiedCount = CountFolders - 1;
					updatePercent();

					lblCurrent.Dispatcher.Invoke(() =>
					{
						lblFile.Text = Messages.CMessages.DataWasBackedUp + "!";
						lblCurrent.Text = "";
					});

					if (!string.IsNullOrEmpty(Email))
					{
						try
						{
							Main.send(m_strEmailUser, Email, "MyBackup - abgeschlossen", Main.buildEmailBody(Messages.CMessages.Salutation,
										Messages.CMessages.BackupFinished + ".", Messages.CMessages.Valediction), m_blnHTML,
										m_strEmailHost, m_intEmailPort, m_strEmailUser, m_strEmailPasswort, m_blnEmailSSL);
						}
						catch (Exception ex) { Log.Log.extendLog(m_strLogName, Messages.CMessages.EmailNotSent + ": " + ex.Message); }
					}
				}));
			}



			Application.Current.Dispatcher.Invoke(() => Close());

			return "";
		}

		private void updatePercent()
		{
			CopiedCount += 1;

			lblPercent.Dispatcher.Invoke(() => lblPercent.Text = Convert.ToInt32(((decimal)CopiedCount / CountFolders) * 100).ToString() + " %",
				DispatcherPriority.Normal);
		}

		private void updateFile(string pv_strFile)
		{
			lblCurrent.Dispatcher.Invoke(() => lblCurrent.Text = string.Format("'{0}' ", pv_strFile),
				DispatcherPriority.Normal);
		}

		private void countChildFolders(string pv_strFolder)
		{
			CountFolders += 1;

			foreach (var strFolder in Directory.GetDirectories(pv_strFolder))
			{
				try
				{
					countChildFolders(strFolder);
				}
				catch (Exception ex)
				{
					Log.Log.extendLog(m_strLogName, "MyBackup.countChildFolders: " + ex.Message);
				}
			}
		}

		private void moveContent(string pv_strFolder)
		{
			if (Directory.Exists(pv_strFolder))
			{
				moveFiles(pv_strFolder);

				foreach (var strFolder in Directory.GetDirectories(pv_strFolder))
				{
					try
					{
						moveContent(strFolder);

						updatePercent();
					}
					catch (Exception ex)
					{
						Log.Log.extendLog(m_strLogName, "MyBackup.moveContent: " + ex.Message);
					}
				}
			}
		}

		private void moveFiles(string pv_strFolder)
		{
			foreach (var strFile in Directory.GetFiles(pv_strFolder))
			{
				try
				{
					var arrFilePath = strFile.Split(@"\".ToCharArray());
					var strTargetPath = Path.Combine(TargetFolder, strFile.Substring(0, strFile.Length - arrFilePath[arrFilePath.GetLength(0) - 1].Length).Replace(":", ""));

					if (!Directory.Exists(strTargetPath)) Directory.CreateDirectory(strTargetPath);

					strTargetPath = Path.Combine(strTargetPath, arrFilePath[arrFilePath.GetLength(0) - 1]);

					updateFile(arrFilePath[arrFilePath.GetLength(0) - 1]);

					if (!File.Exists(strTargetPath))
					{
						File.Copy(strFile, strTargetPath);
					}
					else
					{
						var fiTarget = new FileInfo(strTargetPath);
						var fiSource = new FileInfo(strFile);

						if (fiSource.LastWriteTime > fiTarget.LastWriteTime)
						{
							fiTarget = null;
							fiSource = null;
							File.Delete(strTargetPath);
							File.Copy(strFile, strTargetPath, true);
						}
					}
				}
				catch (Exception ex) { Log.Log.extendLog(m_strLogName, "MyBackup.moveFiles - " + ex.Message); }
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}