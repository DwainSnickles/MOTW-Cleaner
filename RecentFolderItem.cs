using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MOTW_Cleaner
{
    public class RecentFolderItem : RibbonOrbRecentItem
    {
        public string FullPath { get; set; }
    }
}
