using MyBackupModel.Data;
using MyBackupSettings.Classes;
using MyBackupSettings.Classes.Google;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyBackupSettings
{
	public partial class MainWindow : Window
	{
		private decimal m_decBackupSpace;
		CGoogleWebBrowser m_wbBrowser;
		private Task m_tskTreeView;
		//private Task m_tskBackupSpace;
		private decimal backupSpace
		{
			get
			{
				return m_decBackupSpace;
			}
			set
			{
				m_decBackupSpace = value;

				//lblBackupCapacity.Dispatcher.Invoke(() => { lblBackupCapacity.Text = string.Format("Ihr Backup belegt {0} GB", m_decBackupSpace.ToString("N2")); });
			}
		}

		public MainWindow()
		{
			InitializeComponent();
		}

		#region Events

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				//tbxGoogleUser.IsEnabled = false;
				//tbxGooglePassword.IsEnabled = false;

				CSettings.loadDB();
				toggleIntervalStackPanels();
				setupRadioButtons();
				toggleBorderRadioButtons();
				loadTreeView();
				fillFields();
			}
			catch (Exception ex)
			{
				MessageBox.Show(Messages.CErrors.ErrorAtBeginningLong + ": " + Environment.NewLine +
								ex.Message, Messages.CErrors.ErrorAtBeginningShort, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void RadioButtonInterval_Changed(object sender, RoutedEventArgs e)
		{
			toggleIntervalStackPanels();
		}

		private void RadioButtonBaseEvent(object sender, RoutedEventArgs e)
		{
			toggleBorderRadioButtons();
		}

		private void ButtonDone_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var strResult = saveSettings();

				if (strResult != "")
				{
					MessageBox.Show(strResult, Messages.CErrors.ErrorSavingSettingsShort,
									MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					MessageBox.Show(Messages.CMessages.SettingsSavedLong,
								Messages.CMessages.SettingsSavedShort, MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(Messages.CErrors.ErrorSavingSettingsLong + ": " + Environment.NewLine + ex.Message,
								Messages.CErrors.ErrorAtBeginningShort, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Target_TextChanged(object sender, TextChangedEventArgs e)
		{
			checkTargetOrdner();
		}

		private void tbxTarget_GotFocus(object sender, RoutedEventArgs e)
		{
			var fdFolder = new System.Windows.Forms.FolderBrowserDialog();
			System.Windows.Forms.DialogResult objResult;

			fdFolder.Description = Messages.CMessages.SelectTargetFolder;
			fdFolder.RootFolder = Environment.SpecialFolder.MyComputer;
			fdFolder.ShowNewFolderButton = true;

			if (Directory.Exists(tbxTarget.Text.Trim())) fdFolder.SelectedPath = tbxTarget.Text.Trim();

			objResult = fdFolder.ShowDialog();

			if ((objResult == System.Windows.Forms.DialogResult.OK)
				|| (objResult == System.Windows.Forms.DialogResult.Yes))
			{
				tbxTarget.Text = fdFolder.SelectedPath;
			}
			else
			{
				fdFolder.Dispose();
				fdFolder = null;
			}

			rdbMonthBegin.Focus();
		}

		private void Manual_Click(object sender, RoutedEventArgs e)
		{
			var strResult = saveSettings();

			if (strResult != "")
			{
				MessageBox.Show(strResult, Messages.CErrors.ErrorSavingSettingsShort, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				try
				{
					Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyBackupWorker.exe"));
				}
				catch (Exception ex)
				{
					MessageBox.Show(Messages.CErrors.ErrorBackupWorkerLong + ":" + Environment.NewLine + ex.Message,
										Messages.CErrors.ErrorBackupWorkerLong, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void RemoveFolder(object sender, MouseButtonEventArgs e)
		{
			var bFolder = (Border)((Grid)((Border)sender).Parent).Parent;
			var spCon = (StackPanel)bFolder.Parent;

			spCon.Children.Remove(bFolder);

			updateBackupSpace();
		}

		private void tbxTime_LostFocus(object sender, RoutedEventArgs e)
		{
			var objTime = Classes.Helpers.CHelp.getTimeStamp(tbxTime.Text.Trim());

			if ((objTime.Hour < 0) || (objTime.Minute < 0))
			{
				MessageBox.Show(Messages.CErrors.SpecifiedTimeIncorrectLong + "." + Environment.NewLine + Messages.CMessages.EnterTimeFormat + ".",
								Messages.CErrors.SpecifiedTimeIncorrectShort, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		private void fillFields()
		{
			var objSettings = CSettings.getSettings();
			string strTime;

			rdbMonthBegin.IsChecked = objSettings.MonthBegin;
			rdbMonthEnd.IsChecked = objSettings.MonthEnd;
			rdbNoRepeat.IsChecked = !objSettings.Repeat;
			rdbRepeat.IsChecked = objSettings.Repeat;
			rdbRepeatDaily.IsChecked = (objSettings.Repeat && objSettings.Daily);
			rdbRepeatDay.IsChecked = (objSettings.Repeat && objSettings.Day);
			rdbRepeatMonth.IsChecked = (objSettings.Repeat && objSettings.Month);
			rdbRepeatMonthly.IsChecked = (objSettings.Repeat && objSettings.Monthly);
			rdbRepeatWeek.IsChecked = (objSettings.Repeat && objSettings.Week);
			rdbRepeatWeekly.IsChecked = (objSettings.Repeat && objSettings.Weekly);

			if (objSettings.Interval > 0)
			{
				tbxInterval.Text = objSettings.Interval.ToString();
			}
			else
			{
				tbxInterval.Text = "";
			}

			strTime = ("00" + objSettings.Hour.ToString());

			tbxTime.Text = strTime.Substring(strTime.Length - 2, 2) + ":";

			strTime = ("00" + objSettings.Minute.ToString());

			tbxTime.Text += strTime.Substring(strTime.Length - 2, 2);

			tbxTarget.Text = objSettings.TargetPath;
			tbxEmail.Text = objSettings.Email;
			tbxGoogleUser.Text = objSettings.GoogleUser;
			tbxGooglePassword.Text = objSettings.GooglePassword;

			foreach (var objFolder in objSettings.AddedFolders)
			{
				AddBackupFolder(objFolder.Name);
			}
		}

		private string saveSettings()
		{
			var objTime = Classes.Helpers.CHelp.getTimeStamp(tbxTime.Text.Trim());
			var strResult = "";

			if ((objTime.Hour > -1) && (objTime.Minute > -1))
			{
				short shtInterval = 0;
				var lstFolders = CSettings.getFoldersTemplate();

				foreach (var strFolder in getAddedFolders())
				{
					lstFolders.Add(new MyBackupModel.Data.Models.Folder() { Name = strFolder });
				}

				try { shtInterval = Convert.ToInt16(tbxInterval.Text.Trim()); } catch (Exception) { }

				try
				{
					CSettings.saveSettings(rdbMonthBegin.IsChecked, rdbMonthEnd.IsChecked, rdbRepeat.IsChecked,
										rdbRepeatDaily.IsChecked, rdbRepeatDay.IsChecked, rdbRepeatMonth.IsChecked,
										rdbRepeatMonthly.IsChecked, rdbRepeatWeek.IsChecked, rdbRepeatWeekly.IsChecked,
										shtInterval, tbxTarget.Text.Trim(), objTime.Hour, objTime.Minute, tbxEmail.Text.Trim(),
										tbxGoogleUser.Text.Trim(), tbxGooglePassword.Text.Trim(), lstFolders);
				}
				catch (Exception ex)
				{
					strResult = Messages.CErrors.ErrorSavingSettingsLong + ": " + Environment.NewLine + ex.Message;
				}

				if (string.IsNullOrEmpty(strResult))
				{
					try
					{
						CWindowsTask.saveTask();
					}
					catch (Exception ex)
					{
						strResult = Messages.CErrors.ErrorTaskLong + ": " + Environment.NewLine + ex.Message;
					}
				}

				if ((!string.IsNullOrEmpty(tbxGoogleUser.Text)) && (!string.IsNullOrEmpty(tbxGooglePassword.Text)))
				{
					try
					{
						var arrEvents = Classes.Google.CGoogleEvents.getEvents(tbxGoogleUser.Text.Trim(), tbxGooglePassword.Text.Trim());
					}
					catch (Exception ex)
					{
						strResult = (strResult + Environment.NewLine + Messages.CErrors.ErrorGoogleLong + ": " + Environment.NewLine + ex.Message).Trim();
					}
				}
			}
			else
			{
				strResult = Messages.CErrors.SpecifiedTimeIncorrectLong + "." + Environment.NewLine + Messages.CMessages.EnterTimeFormat;
			}

			return strResult;
		}

		/// <summary>
		/// Adds the checked event to toggle the surrounding border of all RadioButtons
		/// </summary>
		private void setupRadioButtons()
		{
			foreach (RadioButton rdbCurrent in Classes.Helpers.Visual.Window.FindVisualChildren<RadioButton>(this))
			{
				rdbCurrent.Checked -= RadioButtonBaseEvent;
				rdbCurrent.Checked += RadioButtonBaseEvent;
			}
		}

		#region Visual Toggles
		/// <summary>
		/// Toggles the interval area in the 'Wie oft' section
		/// </summary>
		private void toggleIntervalStackPanels()
		{
			if (rdbRepeat.IsChecked == true)
			{
				spIntervalA.Visibility = Visibility.Visible;
				spIntervalB.Visibility = Visibility.Visible;
			}
			else
			{
				{
					spIntervalA.Visibility = Visibility.Collapsed;
					spIntervalB.Visibility = Visibility.Collapsed;
				}
			}

			tbxInterval.IsEnabled = !((rdbRepeatDaily.IsChecked == true) || (rdbRepeatMonthly.IsChecked == true) || (rdbRepeatWeekly.IsChecked == true));
		}

		/// <summary>
		/// Toggles the surrounding border of the all RadioButtons
		/// </summary>
		private void toggleBorderRadioButtons()
		{
			Classes.Helpers.Visual.Window.updateBorderRadioButtons(this);
		}
		#endregion

		#region TreeView

		#region Methods
		private void loadTreeView()
		{
			//TreeViewItem tvItem = new TreeViewItem() { Header = System.Security.Principal.WindowsIdentity.GetCurrent().Name };

			foreach (string strPath in Environment.GetLogicalDrives())
			{
				//tvBackup.Items.Add(new TreeViewItem()
				//{
				//	Header = strPath,
				//	Tag = strPath
				//	//Name = strPath
				//});

				tvBackup.Items.Add(buildTreeViewItem(strPath));
			}

			m_tskTreeView = fillTreeView();
		}

		private TreeView getTreeView()
		{
			return Application.Current.Dispatcher.Invoke(() => { return tvBackup; });
		}

		private TreeViewItem buildTreeViewItem(string pv_strPath)
		{
			return Application.Current.Dispatcher.Invoke(() =>
			{
				return buildTreeViewItem(pv_strPath, new string[1] { "lädt..." });
			});
		}

		private TreeViewItem buildTreeViewItem(string pv_strPath, IEnumerable<object> pv_objSource)
		{
			var arrPath = pv_strPath.Split(@"\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var strLastFolder = arrPath[arrPath.GetLength(0) - 1];

			if (arrPath.Length == 1) strLastFolder = pv_strPath;

			return Application.Current.Dispatcher.Invoke(() =>
			{
				var tvCurrentItem = new TreeViewItem()
				{
					Header = strLastFolder,
					ItemsSource = pv_objSource,
					Tag = pv_strPath,
					ToolTip = string.Format(Messages.CMessages.DoubleKlickToAddFolder, pv_strPath)
				};

				tvCurrentItem.MouseDoubleClick -= TreeViewItem_DoubleClick;
				tvCurrentItem.MouseDoubleClick += TreeViewItem_DoubleClick;
				tvCurrentItem.Expanded -= TreeViewItem_Expanded;
				tvCurrentItem.Expanded += TreeViewItem_Expanded;
				tvCurrentItem.Collapsed -= TreeViewItem_Collapsed;
				tvCurrentItem.Collapsed += TreeViewItem_Collapsed;

				return tvCurrentItem;
			});
		}

		private string getTreeViewItemTag(TreeViewItem pv_tvItem)
		{
			return Application.Current.Dispatcher.Invoke(() => { return Convert.ToString(pv_tvItem.Tag); });
		}

		private async Task<string> fillTreeView()
		{
			await Task.Run(new Action(() =>
			{
				TreeView tvCurrent = getTreeView();

				foreach (TreeViewItem tviItem in tvCurrent.Items)
				{
					var lstItems = buildTreeFolder(getTreeViewItemTag(tviItem), false);

					Application.Current.Dispatcher.BeginInvoke(new Action(() => { tviItem.ItemsSource = lstItems; }));
				}
			}));

			return "";
		}

		private List<TreeViewItem> buildTreeFolder(string pv_strParentDirectory, bool pv_blnRecursive)
		{
			var arrTreeItems = new List<TreeViewItem>();
			var diCurrent = new DirectoryInfo(pv_strParentDirectory);

			try
			{
				var dsRights = diCurrent.GetAccessControl();

				if (pv_blnRecursive)
				{
					foreach (DirectoryInfo diFolder in diCurrent.GetDirectories())
					{
						if ((diFolder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
						{
							arrTreeItems.Add(buildTreeViewItem(diFolder.FullName,
																buildTreeFolder(diFolder.FullName, pv_blnRecursive)));
						}
					}
				}
				else
				{
					foreach (DirectoryInfo diFolder in diCurrent.GetDirectories())
					{
						if ((diFolder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
						{
							arrTreeItems.Add(buildTreeViewItem(diFolder.FullName));
						}
					}
				}
			}
			catch (Exception)
			{
				//string str = "";
			}

			return arrTreeItems;
		}

		private StackPanel GetNearestContainer(UIElement element)
		{
			// Walk up the element tree to the nearest tree view item.
			StackPanel container = element as StackPanel;
			while ((container == null) && (element != null))
			{
				element = VisualTreeHelper.GetParent(element) as UIElement;
				container = element as StackPanel;
			}

			return container;
		}

		private bool CheckDropTarget(StackPanel pv_spTarget)
		{
			return (pv_spTarget.Name == "spAddedFolders");
		}
		#endregion

		#region Events
		Point _lastMouseDown;
		TreeViewItem draggedItem;
		StackPanel _target;

		private void TreeViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			var tvCurrent = (TreeViewItem)sender;

			if (e.OriginalSource.GetType() == typeof(TextBlock))
			{
				var lblHeader = (TextBlock)e.OriginalSource;

				if (Convert.ToString(tvCurrent.Header) == lblHeader.Text)
				{
					AddBackupFolder((string)tvCurrent.Tag);
					e.Handled = true;
				}
			}
		}

		private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
		{
			var tvCurrent = (TreeViewItem)sender;
			var tvOriginal = (TreeViewItem)e.OriginalSource;

			if (tvCurrent == tvOriginal)
			{
				tvCurrent.ItemsSource = buildTreeFolder(Convert.ToString(tvCurrent.Tag), false);

			}
		}

		private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
		{
			var tvCurrent = (TreeViewItem)sender;
			var tvOriginal = (TreeViewItem)e.OriginalSource;

			if (tvCurrent == tvOriginal)
			{

			}
		}

		private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				_lastMouseDown = e.GetPosition(tvBackup);
			}
		}

		private void TreeView_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					Point currentPosition = e.GetPosition(tvBackup);

					if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0)
						|| (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
					{
						draggedItem = (TreeViewItem)tvBackup.SelectedItem;

						if (draggedItem != null)
						{
							DragDropEffects finalDropEffect = DragDrop.DoDragDrop(spAddedFolders, tvBackup.SelectedValue, DragDropEffects.Move);

							if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
							{
								// A Move drop was accepted
								if (_target.Name == "spAddedFolders")
								{
									AddBackupFolder(Convert.ToString(draggedItem.Header));
									_target = null;
									draggedItem = null;
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
				//string str = "";
			}
		}

		private void StackPanel_DragOver(object sender, DragEventArgs e)
		{
			try
			{

				Point currentPosition = e.GetPosition(tvBackup);

				if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
					(Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
				{
					// Verify that this is a valid drop and then store the drop target
					StackPanel item = GetNearestContainer(e.OriginalSource as UIElement);

					if (item != null)
					{
						if (CheckDropTarget(item))
						{
							e.Effects = DragDropEffects.Move;
						}
						else
						{
							e.Effects = DragDropEffects.None;
						}
					}
				}

				e.Handled = true;
			}
			catch (Exception)
			{
				//string str = "";
			}
		}

		private void TreeView_Drop(object sender, DragEventArgs e)
		{
			try
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;

				// Verify that this is a valid drop and then store the drop target
				StackPanel TargetItem = GetNearestContainer(e.OriginalSource as UIElement);

				if (TargetItem != null && draggedItem != null)
				{
					_target = TargetItem;
					e.Effects = DragDropEffects.Move;
				}
			}
			catch (Exception)
			{
				//string str = "";
			}
		}

		private void spAddedFolders_DragEnter(object sender, DragEventArgs e)
		{
			_target = (StackPanel)sender;
		}
		#endregion
		#endregion

		private Border buildAddedFolder(string pv_strFolderPath)
		{
			//<Border Style="{StaticResource AddFolderBorder}">
			//< Grid Style = "{StaticResource AddFolderCon}" >
			//             < Grid.ColumnDefinitions >
			//                 < ColumnDefinition Width = "*" ></ ColumnDefinition >
			//                  < ColumnDefinition Width = "50" ></ ColumnDefinition >
			//               </ Grid.ColumnDefinitions >
			//               < TextBlock Grid.Column = "0" Style = "{StaticResource AddFolderText}" Text = "C:\Users\Terry\Downloads\hammerhead-lmy48m\image" ></ TextBlock >
			//              < Border Style = "{StaticResource bL1}"  Grid.Column = "1" >
			//                     < TextBlock Text = "X" Style = "{StaticResource Title}" ></ TextBlock >
			//                 </ Border >
			//             </ Grid >
			//</Border>

			var brBorderCon = new Border() { Style = this.FindResource("AddFolderBorder") as Style };
			var gdFolder = new Grid() { Style = this.FindResource("AddFolderCon") as Style };
			var brBorderX = new Border() { Style = this.FindResource("BorderBlack") as Style };
			var lblX = new TextBlock() { Text = "X", Style = this.FindResource("TitleWhite") as Style, ToolTip = Messages.CMessages.DeleteFolder };

			gdFolder.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			gdFolder.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
			gdFolder.Children.Add(new TextBlock() { Text = pv_strFolderPath, Style = this.FindResource("AddFolderText") as Style });

			Grid.SetColumn(lblX, 1);

			brBorderX.Child = lblX;

			Grid.SetColumn(brBorderX, 1);

			brBorderX.MouseDown -= RemoveFolder;
			brBorderX.MouseDown += RemoveFolder;

			gdFolder.Children.Add(brBorderX);

			brBorderCon.Child = gdFolder;

			return brBorderCon;
		}

		private void AddBackupFolder(string pv_strFolderPath)
		{
			if (!FolderExists(pv_strFolderPath))
			{
				spAddedFolders.Children.Add(buildAddedFolder(pv_strFolderPath));
				updateBackupSpace();
			}
		}

		private bool FolderExists(string pv_strFolderPath)
		{
			pv_strFolderPath = pv_strFolderPath.ToLower();

			foreach (string strFolder in getAddedFolders())
			{
				if (strFolder.ToLower() == pv_strFolderPath) return true;
			}

			return false;
		}

		private List<string> getAddedFolders()
		{
			//<Border Style="{StaticResource AddFolderBorder}">
			//< Grid Style = "{StaticResource AddFolderCon}" >
			//             < Grid.ColumnDefinitions >
			//                 < ColumnDefinition Width = "*" ></ ColumnDefinition >
			//                  < ColumnDefinition Width = "50" ></ ColumnDefinition >
			//               </ Grid.ColumnDefinitions >
			//               < TextBlock Grid.Column = "0" Style = "{StaticResource AddFolderText}" Text = "C:\Users\Terry\Downloads\hammerhead-lmy48m\image" ></ TextBlock >
			//              < Border Style = "{StaticResource bL1}"  Grid.Column = "1" >
			//                     < TextBlock Text = "X" Style = "{StaticResource Title}" ></ TextBlock >
			//                 </ Border >
			//             </ Grid >
			//</Border>
			var lstFolders = new List<string>();

			foreach (Border bChild in spAddedFolders.Children)
			{
				if (bChild.Child.GetType() == typeof(Grid))
				{
					foreach (var objChild in ((Grid)bChild.Child).Children)
					{
						if (objChild.GetType() == typeof(TextBlock))
						{
							var lblChild = (TextBlock)objChild;

							if (lblChild.Style == this.FindResource("AddFolderText") as Style) lstFolders.Add(lblChild.Text);
						}
					}
				}
			}

			return lstFolders;
		}

		private void checkTargetOrdner()
		{
			if (string.IsNullOrEmpty(tbxTarget.Text.Trim()))
			{
				tbxTarget.Text = Messages.CMessages.EnterTargetFolder;
			}
			else if (tbxTarget.Text.Trim() != Messages.CMessages.EnterTargetFolder)
			{
				if (!Directory.Exists(tbxTarget.Text.Trim()))
				{
					MessageBox.Show(Messages.CMessages.TargetFolderNotFound + ":" + Environment.NewLine + Messages.CMessages.CheckEntries + ".",
									Messages.CMessages.TargetFolder, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void updateBackupSpace()
		{
			var lstAddedFolders = getAddedFolders();

			if (lstAddedFolders.Count > 0) backupSpace = lstAddedFolders.Count;
		}

		private void getCredentials(string pv_strID, string pv_strSecret)
		{
			if ((!string.IsNullOrEmpty(pv_strID)) && (!string.IsNullOrEmpty(pv_strSecret)))
			{
				tbxGoogleUser.Dispatcher.Invoke(() =>
				{
					tbxGoogleUser.Text = pv_strID;
					tbxGooglePassword.Text = pv_strSecret;
					wfhCon.Visibility = Visibility.Collapsed;
					wfhCon.Child.Visible = false;
					m_wbBrowser.close();
					m_wbBrowser = null;
				});
			}
		}

		private void showGoogleBrowser()
		{
			System.Windows.Forms.WebBrowser wbGoogle = (System.Windows.Forms.WebBrowser)wfhCon.Child;

			wfhCon.Visibility = Visibility.Visible;

			m_wbBrowser = new CGoogleWebBrowser(ref wbGoogle, getCredentials);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (m_wbBrowser != null)
			{
				//wfhCon.Child.Width = Convert.ToInt32(this.Width);
				//wfhCon.Child.Height = Convert.ToInt32(this.Height);
			}
		}

		private void tbxGoogleUser_GotFocus(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Wollen Sie Ihre Google-Daten eintragen?", "Google", MessageBoxButton.YesNo) == MessageBoxResult.Yes) showGoogleBrowser();
		}
	}
}