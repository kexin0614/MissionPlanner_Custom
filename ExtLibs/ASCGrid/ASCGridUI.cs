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

        static private GMapOverlay ascroutesOverlay;

        // private List<PointLatLngAlt> ascgrid;

        //航线生成（CreateGrid）所需的参数
        private List<PointLatLngAlt> asclist = new List<PointLatLngAlt>();
        private List<PointLatLngAlt> ascgrid;
        private double val_distance;
        private double val_spacing;
        private double val_overshoot1;
        private double val_overshoot2;
        private float val_minLaneSeparation;
        private float val_leadin;
        private double val_adjust;

        //与地图操作有关的临时变量
        private PointLatLng MouseDownStart = new PointLatLng();
        private PointLatLng MouseDownEnd;
        private PointLatLngAlt CurrentGMapMarkerStartPos;
        PointLatLng currentMousePosition;
        GMapMarker CurrentGMapMarker = null;
        int CurrentGMapMarkerIndex = 0;
        bool isMouseDown = false;
        bool isMouseDraging = false;
        static public Object thisLock = new Object();


        public ASCGridUI(ASCGridPlugin plugin)  //构造函数，主要运行的函数
        {
            this.plugin = plugin;

            InitializeComponent();

            ASCComboBoxShutter.Items.AddRange(Enum.GetNames(typeof(ASCChannelCameraShutter)));
            if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane)
            {
                ASCComboBoxTilt.Items.AddRange(Enum.GetNames(typeof(Channelap)));
                ASCComboBoxRoll.Items.AddRange(Enum.GetNames(typeof(Channelap)));
                ASCComboBoxPan.Items.AddRange(Enum.GetNames(typeof(Channelap)));
            }
            else
            {
                ASCComboBoxTilt.Items.AddRange(Enum.GetNames(typeof(Channelac)));
                ASCComboBoxRoll.Items.AddRange(Enum.GetNames(typeof(Channelac)));
                ASCComboBoxPan.Items.AddRange(Enum.GetNames(typeof(Channelac)));
            }

            var points = plugin.Host.FPDrawnPolygon;
            points.Points.ForEach(x => { asclist.Add(x); });  //将多边形信息记录到asclist中
            points.Dispose();

            //更改updown框的值以前，先将响应函数去除，修改后再绑定
            NUM_angle.ValueChanged -= domainUpDown1_ValueChanged;
            NUM_angle.Value = (decimal)((Grid.getAngleOfLongestSide(asclist) + 360) % 360);
            NUM_angle.ValueChanged += domainUpDown1_ValueChanged;

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
        }

        /// <summary>
        /// 用于根据用户设定的数据计算CreateGrid函数所需的参数
        /// </summary>
        private void doCalc()
        {
            double focallen = 6.3;
            double sensorwidth = 6.16;
            double sensorheight = 4.62;

            double flscale = (1000 * (double)NUM_altitude.Value) / focallen;

            double viewwidth = (sensorwidth * flscale / 1000);
            double viewheight = (sensorheight * flscale / 1000);

            double param = (double)NUM_turnparam.Value;

            val_spacing = (1 - ((double)num_overlap.Value / 100.0f)) * viewheight;
            val_distance = (1 - ((double)num_sidelap.Value / 100.0f)) * viewwidth;

            val_overshoot1 = param * 1.5;
            val_overshoot2 = param * 1.5;
            val_minLaneSeparation = 0.0f;
            val_leadin = (float)param * 1.5f;
            val_adjust = param;
            return;
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

        private void myGMAP1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownStart = myGMAP1.FromLocalToLatLng(e.X, e.Y);

            if (e.Button == MouseButtons.Left && Control.ModifierKeys != Keys.Alt)
            {
                isMouseDown = true;
                isMouseDraging = false;

                if (CurrentGMapMarkerStartPos != null)
                    CurrentGMapMarkerIndex = asclist.FindIndex(c => c.ToString() == CurrentGMapMarkerStartPos.ToString());
            }
        }

        private void myGMAP1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseDownEnd = myGMAP1.FromLocalToLatLng(e.X, e.Y);

            if (e.Button == MouseButtons.Right) // ignore right clicks
            {
                return;
            }

            if (isMouseDown) // mouse down on some other object and dragged to here.
            {
                if (e.Button == MouseButtons.Left)
                {
                    isMouseDown = false;
                }
                if (!isMouseDraging)
                {
                    if (CurrentGMapMarker != null)
                    {
                        // Redraw polygon
                        //AddDrawPolygon();
                    }
                }
            }
            isMouseDraging = false;
            CurrentGMapMarker = null;
            CurrentGMapMarkerIndex = 0;
            CurrentGMapMarkerStartPos = null;
        }

        private void myGMAP1_MouseMove(object sender, MouseEventArgs e)
        {
            PointLatLng point = myGMAP1.FromLocalToLatLng(e.X, e.Y);
            currentMousePosition = point;

            if (MouseDownStart == point)
                return;

            if (!isMouseDown)
            {
                // update mouse pos display
                //SetMouseDisplay(point.Lat, point.Lng, 0);
            }

            //draging
            if (e.Button == MouseButtons.Left && isMouseDown)
            {
                isMouseDraging = true;

                if (CurrentGMapMarker != null)
                {
                    if (CurrentGMapMarkerIndex == -1)
                    {
                        isMouseDraging = false;
                        return;
                    }

                    PointLatLng pnew = myGMAP1.FromLocalToLatLng(e.X, e.Y);

                    CurrentGMapMarker.Position = pnew;

                    asclist[CurrentGMapMarkerIndex] = new PointLatLngAlt(pnew);
                    domainUpDown1_ValueChanged(sender, e);
                }
                else // left click pan
                {
                    double latdif = MouseDownStart.Lat - point.Lat;
                    double lngdif = MouseDownStart.Lng - point.Lng;

                    try
                    {
                        lock (thisLock)
                        {
                            myGMAP1.Position = new PointLatLng(myGMAP1.Position.Lat + latdif, myGMAP1.Position.Lng + lngdif);
                        }
                    }
                    catch { }
                }
            }
        }

        private void myGMAP1_OnMapZoomChanged()
        {
            if (myGMAP1.Zoom > 0)
            {
                try
                {
                    TRK_zoom.Value = (float)myGMAP1.Zoom;
                }
                catch { }
            }
        }

        private void TRK_zoom_Scroll(object sender, EventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    myGMAP1.Zoom = TRK_zoom.Value;
                }
            }
            catch { }
        }
        
        #endregion

        #region 事件响应函数

        /// <summary>
        /// 用于相应用户更改参数的动作，重新计算航点并绘制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void domainUpDown1_ValueChanged(object sender, EventArgs e)
        {
            doCalc();
            ascgrid = Grid.CreateGrid(asclist, (double)NUM_altitude.Value,
                val_distance, val_spacing, (double)NUM_angle.Value,
                val_overshoot1, val_overshoot2,
                Grid.StartPosition.Home, false,
                val_minLaneSeparation, val_leadin, val_adjust);

            myGMAP1.HoldInvalidation = true;

                ascroutesOverlay.Markers.Clear();
                ascroutesOverlay.Routes.Clear();
                ascroutesOverlay.Polygons.Clear();
            
            if (ascgrid.Count == 0)
            {
                return;
            }

            DrawPolygon();//绘制多边形边界和顶点

            int strips = 0;
            int images = 0;
            int a = 1;
            PointLatLngAlt prevpoint = ascgrid[0];
            float routetotal = 0;
            List<PointLatLng> segment = new List<PointLatLng>();
            double maxgroundelevation = double.MinValue;
            double mingroundelevation = double.MaxValue;
            double startalt = plugin.Host.cs.HomeAlt;

            foreach (var item in ascgrid)
            {
                double currentalt = srtm.getAltitude(item.Lat, item.Lng).alt;
                mingroundelevation = Math.Min(mingroundelevation, currentalt);
                maxgroundelevation = Math.Max(maxgroundelevation, currentalt);

                if (item.Tag == "M")
                {
                    images++;
                }
                else
                {
                    if (item.Tag != "SM" && item.Tag != "ME")
                        strips++;

                    var marker = new GMapMarkerWP(item, a.ToString()) { ToolTipText = a.ToString(), ToolTipMode = MarkerTooltipMode.OnMouseOver };
                    ascroutesOverlay.Markers.Add(marker);

                    segment.Add(prevpoint);
                    segment.Add(item);
                    prevpoint = item;
                    a++;
                }

                GMapRoute seg = new GMapRoute(segment, "segment" + a.ToString());
                seg.Stroke = new Pen(Color.Yellow, 4);
                seg.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                seg.IsHitTestVisible = true;
                routetotal = routetotal + (float)seg.Distance;

                ascroutesOverlay.Routes.Add(seg);
                segment.Clear();

            }
            
            myGMAP1.HoldInvalidation = false;
            if (!isMouseDown)
                myGMAP1.ZoomAndCenterMarkers(ascroutesOverlay.Id);

            myGMAP1.Invalidate();
        }

        private void ASCGridUI_Load(object sender, EventArgs e)
        {
            myGMAP1.MapProvider = plugin.Host.FDMapType;   //选择host使用的地图提供商

            ascroutesOverlay = new GMapOverlay("ascroutesOverlay");  //id为ascroutesOverlay的路径图层
            myGMAP1.Overlays.Add(ascroutesOverlay);  //增加图层

            myGMAP1.OnPositionChanged += myGMAP1_OnCurrentPositionChanged; //运行myGMAP1_OnCurrentPositionChanged函数
            myGMAP1.Overlays.Add(FlightPlanner.airportsoverlay); //增加机场

            TRK_zoom.Value = (float)myGMAP1.Zoom;

            domainUpDown1_ValueChanged(this, null);//绘制航点
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
