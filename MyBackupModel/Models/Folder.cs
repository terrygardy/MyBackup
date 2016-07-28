using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBackupModel.Data.Models
{
	public class Folder
	{
		public string Header { get; set; }
		public string Name { get; set; }

		public Folder() { Name = ""; Header = ""; }
	}
}
