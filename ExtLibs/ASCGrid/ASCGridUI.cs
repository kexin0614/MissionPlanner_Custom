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
using MissionPlanner.Utilities;
using MissionPlanner.Properties;
using MissionPlanner.Controls.Waypoints;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;


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
            myGMAP1.Position = new GMap.NET.PointLatLng(31.027964, 121.439399);   //致远湖的坐标

            myGMAP1.OnPositionChanged += myGMAP1_OnCurrentPositionChanged;
            myGMAP1.Overlays.Add(FlightPlanner.airportsoverlay);

        }

        void myGMAP1_OnCurrentPositionChanged(PointLatLng point)
        {
            FlightPlanner.airportsoverlay.Clear();
            foreach (var item in Airports.getAirports(myGMAP1.Position))
            {
                FlightPlanner.airportsoverlay.Markers.Add(new GMapMarkerAirport(item)
                {
                    ToolTipText = item.Tag,
                    ToolTipMode = MarkerTooltipMode.OnMouseOver
                });
            }
        }

        private void ASCGridUI_Load(object sender, EventArgs e)
        {

        }

    }
}
