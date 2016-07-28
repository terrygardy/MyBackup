using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Calendar;
using Google.GData.Extensions;
using Google.GData.AccessControl;
using Google.GData.Client;

namespace MyBackupSettings.Classes.Google
{
	public class Calendar
	{
		public DateTime Date { get; set; }
		public string Title { get; set; }
	}

	public class CalendarLogin
	{
		public string URL { get; set; }
		public string ID { get; set; }

		public CalendarLogin(string pv_URL, string pv_strID)
		{
			URL = pv_URL;
			ID = pv_strID;
		}
	}

	public class CGoogleEvents
	{
		private static readonly string CAL_TEMPLATE = "https://www.google.com/calendar/feeds/{0}/private/full";
		//private static readonly string CalUrl = "https://www.google.com/calendar/feeds/default/private/full";

		public static Calendar[] getEvents(string pv_strUserName, string pv_strPassword)
		{
			return getEvents(pv_strUserName, pv_strPassword, "MyBackup");
		}

		public static Calendar[] getEvents(string pv_strUserName, string pv_strPassword, string pv_strCalendarName)
		{
			var csCalService = new CalendarService("Calendar");

			try
			{
				var clLogin = googleAuthentication(pv_strUserName, pv_strPassword, pv_strCalendarName, ref csCalService);

				if (clLogin != null)
				{
					EventQuery eqQuery = new EventQuery(clLogin.URL);
					EventFeed efFeed = csCalService.Query(eqQuery);

					return (from EventEntry entry in efFeed.Entries
							select new Calendar()
							{
								Date = entry.Times[0].StartTime,
								Title = entry.Title.Text
							}).ToArray();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(Messages.CErrors.ErrorGoogleEvents + ": " + ex.Message, ex);
			}

			return new Calendar[0];
		}

		private static CalendarLogin googleAuthentication(string pv_strUserName, string pv_strPassword, string pv_strCalendarName, ref CalendarService pr_csService)
		{
			pr_csService.setUserCredentials(pv_strUserName, pv_strPassword);

			return saveCalIdAndUrl(pv_strCalendarName, ref pr_csService);
		}

		private static CalendarLogin saveCalIdAndUrl(string pv_strCalendarName, ref CalendarService pr_csService)
		{
			CalendarQuery cqQuery = new CalendarQuery() { Uri = new Uri(string.Format(CAL_TEMPLATE, pr_csService.Credentials.Username)) };
			CalendarFeed cfResultFeed = pr_csService.Query(cqQuery);

			foreach (CalendarEntry ceEntry in cfResultFeed.Entries)
			{
				if (ceEntry.Title.Text == pv_strCalendarName)
				{
					var strCalId = ceEntry.Id.AbsoluteUri.Substring(63);

					return new CalendarLogin(string.Format(CAL_TEMPLATE, strCalId), strCalId);
				}
			}

			return null;
		}
	}
}
