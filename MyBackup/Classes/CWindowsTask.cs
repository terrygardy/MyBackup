using Microsoft.Win32.TaskScheduler;
using MyBackupModel.Data;
using System;

namespace MyBackupSettings.Classes
{
	public class CWindowsTask
	{
		public static void saveTask()
		{
			var objSettings = CSettings.getSettings();
			var tsService = new TaskService();
			var datStart = DateTime.Now;
			var strTaskName = "MyBackup";
			string strWorkerPath;
			TaskDefinition tdNew;

			// Remove the task we just created
			tsService.RootFolder.DeleteTask(strTaskName, false);

			// Create a new task definition and assign properties
			tdNew = tsService.NewTask();
			tdNew.RegistrationInfo.Description = Messages.CMessages.TaskDescription;

			if (objSettings.MonthBegin)
			{
				datStart = new DateTime(datStart.Year, datStart.Month, 1, objSettings.Hour, objSettings.Minute, 0);

				while (datStart < DateTime.Now) datStart = datStart.AddMonths(1);
			}
			else
			{
				datStart = new DateTime(datStart.Year, datStart.Month, 1, objSettings.Hour, objSettings.Minute, 0).AddDays(-1);

				while (datStart < DateTime.Now)
				{
					datStart = datStart.AddMonths(2);
					datStart = new DateTime(datStart.Year, datStart.Month, 1, objSettings.Hour, objSettings.Minute, 0).AddDays(-1);
				}
			}

			// Create a trigger that will fire the task at the given time
			if (objSettings.Repeat)
			{
				if (objSettings.Daily)
				{
					tdNew.Triggers.Add(getDailyTrigger(datStart, 1));
				}
				else if (objSettings.Day)
				{
					tdNew.Triggers.Add(getDailyTrigger(datStart, objSettings.Interval));
				}
				else if (objSettings.Weekly)
				{
					tdNew.Triggers.Add(getWeeklyTrigger(datStart, 1));
				}
				else if (objSettings.Week)
				{
					tdNew.Triggers.Add(getWeeklyTrigger(datStart, objSettings.Interval));
				}
				else if (objSettings.Monthly)
				{
					tdNew.Triggers.Add(getMonthlyTrigger(datStart, objSettings.MonthEnd));
				}
				else if (objSettings.Month)
				{
					if (objSettings.Interval == 1)
					{
						tdNew.Triggers.Add(getMonthlyTrigger(datStart, objSettings.MonthEnd));
					}
					else if (objSettings.Interval > 0)
					{
						var i = 1;

						do
						{
							tdNew.Triggers.Add(getMonthlyTrigger(datStart, objSettings.MonthEnd, i));
							i += objSettings.Interval;
						}
						while (i < 13);
					}
					else
					{
						tdNew.Triggers.Add(getMonthlyTrigger(datStart, objSettings.MonthEnd, datStart.Month));
					}
				}
			}
			else
			{
				tdNew.Triggers.Add(new TimeTrigger(datStart));
			}

			// Create an action that will launch the MyBackupWorker whenever the trigger fires
			strWorkerPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyBackupWorker.exe");

			if (!System.IO.File.Exists(strWorkerPath))
				throw new Exception(Messages.CErrors.BackupFailed + "." + Environment.NewLine +
									 Messages.CErrors.BackupWorkerNotFound + ": " + strWorkerPath);

			tdNew.Actions.Add(new ExecAction(strWorkerPath, "", null));

			// Register the task in the root folder
			tsService.RootFolder.RegisterTaskDefinition(strTaskName, tdNew);
		}

		private static MonthlyTrigger getMonthlyTrigger(DateTime pv_datStart, bool pv_blnMonthEnd)
		{
			return getMonthlyTrigger(pv_datStart, pv_blnMonthEnd, getMonthOfYear(0));
		}

		private static MonthlyTrigger getMonthlyTrigger(DateTime pv_datStart, bool pv_blnMonthEnd, int pv_intMonth)
		{
			return getMonthlyTrigger(pv_datStart, pv_blnMonthEnd, getMonthOfYear(pv_intMonth));
		}

		private static MonthlyTrigger getMonthlyTrigger(DateTime pv_datStart, bool pv_blnMonthEnd, MonthsOfTheYear pv_objMonthOfYear)
		{
			if (pv_blnMonthEnd)
			{
				return new MonthlyTrigger()
				{
					MonthsOfYear = pv_objMonthOfYear,
					RunOnLastDayOfMonth = pv_blnMonthEnd,
					DaysOfMonth = new int[0],
					StartBoundary = new DateTime(pv_datStart.Year, pv_datStart.Month, pv_datStart.Day, pv_datStart.Hour, pv_datStart.Minute, 0)
				};
			}
			else
			{
				return new MonthlyTrigger()
				{
					MonthsOfYear = pv_objMonthOfYear,
					DaysOfMonth = new int[1] { 1 },
					StartBoundary = new DateTime(pv_datStart.Year, pv_datStart.Month, pv_datStart.Day, pv_datStart.Hour, pv_datStart.Minute, 0)
				};
			}
		}

		private static MonthsOfTheYear getMonthOfYear(int pv_intMonth)
		{
			switch (pv_intMonth)
			{
				case 1: return MonthsOfTheYear.January;
				case 2: return MonthsOfTheYear.February;
				case 3: return MonthsOfTheYear.March;
				case 4: return MonthsOfTheYear.April;
				case 5: return MonthsOfTheYear.May;
				case 6: return MonthsOfTheYear.June;
				case 7: return MonthsOfTheYear.July;
				case 8: return MonthsOfTheYear.August;
				case 9: return MonthsOfTheYear.September;
				case 10: return MonthsOfTheYear.October;
				case 11: return MonthsOfTheYear.November;
				case 12: return MonthsOfTheYear.December;
			}

			return MonthsOfTheYear.AllMonths;
		}

		private static WeeklyTrigger getWeeklyTrigger(DateTime pv_datStart, short pv_shtInterval)
		{
			return new WeeklyTrigger()
			{
				WeeksInterval = pv_shtInterval,
				StartBoundary = pv_datStart
			};
		}

		private static DailyTrigger getDailyTrigger(DateTime pv_datStart, short pv_shtInterval)
		{
			return new DailyTrigger()
			{
				DaysInterval = pv_shtInterval,
				StartBoundary = pv_datStart
			};
		}
	}
}