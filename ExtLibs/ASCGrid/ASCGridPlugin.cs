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

            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ASCGridUI));
            //var temp = (string)(resources.GetObject("$this.Text"));

            but = new ToolStripMenuItem("ASCGrid");  //按钮名称为ASCGrid
            but.Click += but_Click;  //执行but_Click，这个+=的用法还不是很理解

            bool hit = false; 
            ToolStripItemCollection col = Host.FPMenuMap.Items;  //应该是右键选项列表中的选项
            int index = col.Count;
            foreach (ToolStripItem item in col)
            {
                if (item.Text.Equals(Strings.AutoWP))  //找到AutoWP的选项
                {
                    index = col.IndexOf(item);
                    ((ToolStripMenuItem)item).DropDownItems.Add(but);  //增加ASCGrid的按钮
                    hit = true;
                    break;
                }
            }

            if (hit == false)   //如果没有找到AutoWP的选项，直接在右键列表中增加ASCGrid按钮
                col.Add(but);

            return true;
        }

        void but_Click(object sender, EventArgs e)
        {
            using (Form ascgridui = new ASCGridUI(this))  //实例化ASCGridUI类
            {
                MissionPlanner.Utilities.ThemeManager.ApplyThemeTo(ascgridui);  
                //应该是框的样式之类的
                ascgridui.ShowDialog();//显示
            }
            //if (Host.FPDrawnPolygon != null && Host.FPDrawnPolygon.Points.Count > 2)
            //{
            //    using (Form ascgridui = new ASCGridUI(this))
            //    {
            //        MissionPlanner.Utilities.ThemeManager.ApplyThemeTo(ascgridui);
            //        ascgridui.ShowDialog();
            //    }
            //}
            //else
            //{
            //    CustomMessageBox.Show("Please define a polygon.", "Error");
            //}
        }

        public override bool Exit()
        {
            return true;
        }
    }
}
