using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyBackupSettings.Classes.Helpers.Visual
{
	public class Window
	{
		/// <summary>
		/// searches for all of the children with the passed type
		/// </summary>
		/// <typeparam name="T">Type of control</typeparam>
		/// <param name="pv_depObj">Parent - Example: Window</param>
		/// <returns></returns>
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject pv_depObj) where T : DependencyObject
		{
			if (pv_depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(pv_depObj); i++)
				{
					DependencyObject doChild = VisualTreeHelper.GetChild(pv_depObj, i);

					if (doChild != null && doChild is T) yield return (T)doChild;

					foreach (T objChildOfChild in FindVisualChildren<T>(doChild))
					{
						yield return objChildOfChild;
					}
				}
			}
		}

		/// <summary>
		/// Toggles the surrounding border of the all RadioButtons
		/// </summary>
		public static void updateBorderRadioButtons(System.Windows.Window pv_objWindow)
		{
			foreach (RadioButton rdbCurrent in FindVisualChildren<RadioButton>(pv_objWindow))
			{
				if (rdbCurrent.Parent.GetType() == typeof(Border))
				{
					if (rdbCurrent.IsChecked == false)
					{
						((Border)rdbCurrent.Parent).Style = pv_objWindow.FindResource("NoBorderRdb") as Style;
					}
					else
					{
						((Border)rdbCurrent.Parent).Style = pv_objWindow.FindResource("StandardBorderRdb") as Style;
					}
				}
			}
		}
	}
}
