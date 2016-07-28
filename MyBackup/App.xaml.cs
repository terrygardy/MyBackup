using MyBackupModel.Data;
using System.Windows;

namespace MyBackupSettings
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{ }

		protected override void OnExit(ExitEventArgs e)
		{
			CSettings.closeDB();
		}
	}
}