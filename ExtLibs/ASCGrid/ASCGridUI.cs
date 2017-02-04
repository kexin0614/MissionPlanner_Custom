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
using MissionPlanner.Controls;
using MissionPlanner.Controls.Waypoints;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;


namespace MissionPlanner
{
    public partial class ASCGridUI : Form
    {
        private ASCGridPlugin plugin; //plugin可供调用主窗口的控件、数据和设置

        static GMapOverlay ascroutesOverlay;    

        // private List<PointLatLngAlt> ascgrid;
        private List<PointLatLngAlt> asclist = new List<PointLatLngAlt>();            



        //航线生成所需的



        public ASCGridUI(ASCGridPlugin plugin)  //构造函数，主要运行的函数
        {
            this.plugin = plugin;

            InitializeComponent();

            ASCComboBoxShutter.Items.AddRange(Enum.GetNames(typeof(ASCChannelCameraShutter)));
            if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane)
            {
                ASCComboBoxTilt.Items.AddRange(Enum.GetNames(typeof(Channelap)));
                ASCComboBoxRoll.Items.AddRange(Enum.GetNames(typeof(Channelap)));
                //mavlinkComboBoxPan.Items.AddRange(Enum.GetNames(typeof(Channelap)));
            }
            else
            {
                ASCComboBoxTilt.Items.AddRange(Enum.GetNames(typeof(Channelac)));
                ASCComboBoxRoll.Items.AddRange(Enum.GetNames(typeof(Channelac)));
                //mavlinkComboBoxPan.Items.AddRange(Enum.GetNames(typeof(Channelac)));
            }

        }

        private void FindingASCGrid(object sender, EventArgs e)   //计算航点
        {
            //ASCCreateGrid
        }

        void DrawPolygon()    //绘制多边形边界和顶点
        {
            //ascroutesOverlay.Polygons.Clear();
            //ascroutesOverlay.Markers.Clear();
            List<PointLatLng> asclist2 = new List<PointLatLng>();

            asclist.ForEach(x => { asclist2.Add(x); });

            var ascpoly = new GMapPolygon(asclist2, "poly");
            ascpoly.Stroke = new Pen(Color.Red, 2);
            ascpoly.Fill = Brushes.Transparent;

            ascroutesOverlay.Polygons.Add(ascpoly);

            foreach (var item in asclist)
            {
                ascroutesOverlay.Markers.Add(new GMarkerGoogle(item, GMarkerGoogleType.red));
            }

            myGMAP1.ZoomAndCenterMarkers(ascroutesOverlay.Id);    //调整地图zoom的大小，使得其可以显示所有点
        }


        #region 地图控件相关

        void myGMAP1_OnCurrentPositionChanged(PointLatLng point)  //现有功能是增加机场的Marker
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

        #endregion

        #region 事件响应函数

        private void ASCGridUI_Load(object sender, EventArgs e)
        {
            myGMAP1.MapProvider = plugin.Host.FDMapType;   //选择host使用的地图提供商

            ascroutesOverlay = new GMapOverlay("ascroutesOverlay");  //id为ascroutesOverlay的路径图层
            myGMAP1.Overlays.Add(ascroutesOverlay);  //增加图层

            myGMAP1.OnPositionChanged += myGMAP1_OnCurrentPositionChanged; //运行myGMAP1_OnCurrentPositionChanged函数
            myGMAP1.Overlays.Add(FlightPlanner.airportsoverlay); //增加机场

            var points = plugin.Host.FPDrawnPolygon;
            points.Points.ForEach(x => { asclist.Add(x); });  //将多边形信息记录到asclist中
            points.Dispose();

            FindingASCGrid(this, null);  //调用计算航点函数

            DrawPolygon();//绘制多边形边界和顶点
        }

        private void ASCGridUI_Resize(object sender, EventArgs e)
        {
            myGMAP1.ZoomAndCenterMarkers(ascroutesOverlay.Id);    //在改变窗口大小时，依旧保持zoom和中心在合适位置
        }

        #endregion


        private enum ASCChannelCameraShutter
        {
            Disable = 0,
            RC5 = 5,
            RC6 = 6,
            RC7 = 7,
            RC8 = 8,
            RC9 = 1,
            RC10 = 10,
            RC11 = 11,
            Relay = 1,
            Transistor = 4
        }
        private enum Channelap
        {
            Disable = 0,
            RC5 = 1,
            RC6 = 1,
            RC7 = 1,
            RC8 = 1,
            RC9 = 1,
            RC10 = 1,
            RC11 = 1
        }
        private enum Channelac
        {
            Disable = 0,
            RC5 = 1,
            RC6 = 1,
            RC7 = 1,
            RC8 = 1,
            RC9 = 1,
            RC10 = 1,
            RC11 = 1
        }
    }
}
