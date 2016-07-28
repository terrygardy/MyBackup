using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml;

namespace MyBackupSettings.Classes.Google
{
	public class CGoogleWebBrowser
	{
		private bool m_blnReturned = false;
		private WebBrowser m_wbBrowser;
		private Action<string, string> m_fnRetrieveCredentials;
		private Task<string> m_tSearcher;
		private delegate string SearchDOM();
		private SearchDOM m_fnSearchDOM;

		public CGoogleWebBrowser(ref WebBrowser pr_wbGoogle, Action<string, string> pv_fnCredentials)
		{
			m_wbBrowser = pr_wbGoogle;
			m_tSearcher = null;
			m_fnSearchDOM = searchBody;
			hide();
			setOnCompleted(pv_fnCredentials);
			navigate(@"https://console.developers.google.com");
			show();
			bringToFront();
		}

		public void buildNewBrowser()
		{
			if (m_wbBrowser != null) close();

			m_wbBrowser = new WebBrowser();
		}

		public void setOnCompleted(Action<string, string> pv_fnOnCompleted)
		{
			if (m_wbBrowser != null)
			{
				m_wbBrowser.DocumentCompleted -= OnCompleted;
				m_wbBrowser.DocumentCompleted += OnCompleted;
				m_fnRetrieveCredentials = pv_fnOnCompleted;
			}
		}

		private void OnCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if ((m_fnRetrieveCredentials != null) && (m_tSearcher == null))
			{
				m_tSearcher = startSearcher();
			}
		}

		public void resize(int pv_intWidth, int pv_intHeight)
		{
			if (m_wbBrowser != null)
			{
				m_wbBrowser.Width = pv_intWidth;
				m_wbBrowser.Height = pv_intHeight;
			}
		}

		public void hide() { toggleView(false); }

		public void show() { toggleView(true); }

		private void toggleView(bool pv_blnToggle)
		{
			if (m_wbBrowser != null) m_wbBrowser.Visible = pv_blnToggle;
		}

		public void bringToFront()
		{
			if (m_wbBrowser != null)
			{
				show();
				m_wbBrowser.BringToFront();
			}
		}

		public void navigate(string pv_strURL)
		{
			if (m_wbBrowser != null)
			{
				m_wbBrowser.Navigate(pv_strURL);
			}
		}

		public void close()
		{
			if (m_wbBrowser != null)
			{
				hide();
				m_wbBrowser.Dispose();
				m_tSearcher = null;
			}
		}

		public WebBrowser getBrowser() { return m_wbBrowser; }

		private async Task<string> startSearcher()
		{
			return await Task.Run(() =>
			{
				while (!m_blnReturned)
				{
					string strDocument = (string)m_wbBrowser.Invoke(m_fnSearchDOM);

					findCredentials(strDocument);

					Task.Delay(5000);
				}

				return "";
			});
		}

		private bool m_blnInProgress = false;

		private string searchBody()
		{
			return m_wbBrowser.Document.Body.InnerHtml;
		}

		private void findCredentials(string pv_strHTML)
		{
			if (m_blnInProgress) return;

			m_blnInProgress = true;

			try
			{
				int intIndex = pv_strHTML.ToLower().IndexOf("a title=\"download json\"");

				if (intIndex > -1)
				{
					string strID = "", strSecret = "";
					var arrElements = pv_strHTML.Substring(intIndex).Split("{".ToCharArray());
					string[] arrContent = null;

					foreach (var strElement in arrElements)
					{
						if (strElement.ToLower().Contains("client_id"))
						{
							arrContent = strElement.Split(",".ToCharArray());
							break;
						}
					}

					foreach (var strElement in arrContent)
					{
						if (strElement.ToLower().Contains("client_id"))
						{
							strID = strElement.Split(":".ToCharArray())[1];
							strID = strID.Substring(1, strID.Length - 2);
						}
						else if (strElement.ToLower().Contains("client_secret"))
						{
							strSecret = strElement.Split(":".ToCharArray())[1];
							strSecret = strSecret.Substring(1, strSecret.Length - 2);
						}
					}

					if ((!string.IsNullOrEmpty(strID)) && (!string.IsNullOrEmpty(strSecret))) m_blnReturned = true; m_fnRetrieveCredentials(strID, strSecret);
				}
			}
			catch (Exception ex)
			{
				string str = "";

				str = ex.Message;
			}

			m_blnInProgress = false;
		}
	}
}
