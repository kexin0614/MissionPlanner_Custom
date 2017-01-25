using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET.WindowsForms;
using System.Windows.Forms;

namespace MissionPlanner
{
    public class ASCGridPlugin : MissionPlanner.Plugin.Plugin
    {
       

        ToolStripMenuItem but;

        public override string Name
        {
            get { return "ASCGrid"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        public override string Author
        {
            get { return "Michael Oborne"; }
        }

        public override bool Init()
        {
            return true;
        }

        public override bool Loaded()
        {
            ASCGrid.Host2 = Host;

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ASCGridUI));
            var temp = (string)(resources.GetObject("$this.Text"));

            but = new ToolStripMenuItem(temp);
            but.Click += but_Click;

            bool hit = false;
            ToolStripItemCollection col = Host.FPMenuMap.Items;
            int index = col.Count;
            foreach (ToolStripItem item in col)
            {
                if (item.Text.Equals(Strings.AutoWP))
                {
                    index = col.IndexOf(item);
                    ((ToolStripMenuItem)item).DropDownItems.Add(but);
                    hit = true;
                    break;
                }
            }

            if (hit == false)
                col.Add(but);

            return true;
        }

        void but_Click(object sender, EventArgs e)
        {
            using (var ascgridui = new ASCGridUI(this))
            {
                MissionPlanner.Utilities.ThemeManager.ApplyThemeTo(ascgridui);

                if (Host.FPDrawnPolygon != null && Host.FPDrawnPolygon.Points.Count > 2)
                {
                    ascgridui.ShowDialog();
                }
                else
                {
                    CustomMessageBox.Show("Please define a polygon.", "Error");
                }
            }
            
        }

        public override bool Exit()
        {
            return true;
        }
    }
}
