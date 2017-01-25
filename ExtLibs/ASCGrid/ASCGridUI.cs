using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using MissionPlanner.GCSViews;
using MissionPlanner.Properties;


namespace MissionPlanner
{
    public partial class ASCGridUI : Form
    {
        private ASCGridPlugin plugin;

        public ASCGridUI(ASCGridPlugin plugin)
        {
            this.plugin = plugin;

            InitializeComponent();

            myGMAP1.MapProvider = plugin.Host.FDMapType;
            myGMAP1.Position = new GMap.NET.PointLatLng(31.027964, 121.439399);
        }

        private void ASCGridUI_Load(object sender, EventArgs e)
        {

        }

    }
}
