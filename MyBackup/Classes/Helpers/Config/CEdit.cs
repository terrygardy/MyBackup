using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyBackupSettings.Classes.Helpers.Config
{
	public class CEdit
	{
		public static void updateConnectionString(string pv_strValue)
		{
			updateConfigFile("connectionStrings", pv_strValue);
		}

		private static void updateConfigFile(string pv_strElementName, string pv_strValue)
		{
			XmlDocument xmlDoc = new XmlDocument();

			xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
			pv_strElementName = pv_strElementName.ToLower();

			foreach (XmlElement xElement in xmlDoc.DocumentElement)
			{
				if (xElement.Name.ToLower() == pv_strElementName) xElement.FirstChild.Attributes[2].Value = pv_strValue;
			}

			xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
		}
	}
}
