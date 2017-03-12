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
using System.Diagnostics;
using MissionPlanner.GCSViews;
using MissionPlanner.Utilities;
using MissionPlanner.Properties;
using MissionPlanner.Controls;
using MissionPlanner.GeoRef;
using MissionPlanner.Controls.Waypoints;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Collections;
using MissionPlanner.Log;
using System.IO;
using Microsoft.Win32;

namespace MissionPlanner
{
    public partial class ASCGridUI : Form
    {
        private ASCGridPlugin plugin; //plugin可供调用主窗口的控件、数据和设置

        static private GMapOverlay ascroutesOverlay;

        //航线生成（CreateGrid）所需的参数
        private List<PointLatLngAlt> asclist = new List<PointLatLngAlt>();
        private List<PointLatLngAlt> ascgrid;
        private List<PointLatLngAlt> ascgrid_cam=new List<PointLatLngAlt>();
        private List<ImageInfo> imageinfo = new List<ImageInfo>();
        private double val_distance;
        private double val_spacing;
        private double val_overshoot1;
        private double val_overshoot2;
        private float val_minLaneSeparation;
        private float val_leadin;
        private double val_adjust;
        private Grid.StartPosition startpos;
        private Dictionary<string, PictureInformation> picturesInfo=new Dictionary<string, PictureInformation>();

        //与地图操作有关的临时变量
        private PointLatLng MouseDownStart = new PointLatLng();
        private PointLatLng MouseDownEnd = new PointLatLng();
        private PointLatLngAlt CurrentGMapMarkerStartPos;
        PointLatLng currentMousePosition;
        GMapMarker CurrentGMapMarker = null;
        int CurrentGMapMarkerIndex = 0;
        bool isMouseDown = false;
        bool isMouseDraging = false;
        static public Object thisLock = new Object();
        GMapMarker marker;

        //发送航点所需要的变量
        Locationwp home = new Locationwp();


        public ASCGridUI(ASCGridPlugin plugin)  //构造函数，主要运行的函数
        {
            this.plugin = plugin;

            InitializeComponent();
            comboBox1.DataSource = Enum.GetNames(typeof(Grid.StartPosition));
            comboBox1.SelectedIndex = 0;
            startpos = (Grid.StartPosition)Enum.Parse(typeof(Grid.StartPosition), comboBox1.Text);

            //ASCComboBoxShutter.Items.AddRange(Enum.GetNames(typeof(ASCChannelCameraShutter)));
            //if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane)
            //{
            //    ASCComboBoxTilt.Items.AddRange(Enum.GetNames(typeof(Channelap)));
            //    ASCComboBoxRoll.Items.AddRange(Enum.GetNames(typeof(Channelap)));
            //    ASCComboBoxPan.Items.AddRange(Enum.GetNames(typeof(Channelap)));
            //}
            //else
            //{
            //    ASCComboBoxTilt.Items.AddRange(Enum.GetNames(typeof(Channelac)));
            //    ASCComboBoxRoll.Items.AddRange(Enum.GetNames(typeof(Channelac)));
            //    ASCComboBoxPan.Items.AddRange(Enum.GetNames(typeof(Channelac)));
            //}

            var points = plugin.Host.FPDrawnPolygon;
            points.Points.ForEach(x => { asclist.Add(x); });  //将多边形信息记录到asclist中
            points.Dispose();

            myGMAP1.MapProvider = plugin.Host.FDMapType;   //选择host使用的地图提供商

            ascroutesOverlay = new GMapOverlay("ascroutesOverlay");  //id为ascroutesOverlay的路径图层
            myGMAP1.Overlays.Add(ascroutesOverlay);  //增加图层

            //myGMAP1.Overlays.Add(FlightPlanner.airportsoverlay); //增加机场

            //更改updown框的值以前，先将响应函数去除，修改后再绑定
            NUM_angle.ValueChanged -= domainUpDown1_ValueChanged;
            NUM_angle.Value = (decimal)((Grid.getAngleOfLongestSide(asclist) + 360) % 360);
            NUM_angle.ValueChanged += domainUpDown1_ValueChanged;

        }

        /// <summary>
        /// 绘制多边形边界和顶点
        /// </summary>
        void DrawPolygon()
        {
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

        Locationwp PointLatLngAlttoLocationwp(int a)
        {
            try
            {
                Locationwp temp = new Locationwp();
                temp.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;

                temp.p1 = 0;
                temp.p2 = 0;
                temp.p3 = 0;
                temp.p4 = 0;
                temp.alt =(float)(ascgrid[a].Alt / CurrentState.multiplierdist);
                temp.lat = ascgrid[a].Lat;
                temp.lng = ascgrid[a].Lng;

                return temp;
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid number on waypoint #" + (a + 1).ToString(), ex);
            }
        }

        List<Locationwp> GetCommandList()
        {
            List<Locationwp> commands = new List<Locationwp>();

            for (int a = 0; a < ascgrid.Count - 0; a++)
            {
                if (ascgrid[a].Tag != "M")
                {
                    var temp = PointLatLngAlttoLocationwp(a);
                    commands.Add(temp);
                }
                if (ascgrid[a].Tag == "SM")
                {
                    var temp = new Locationwp();
                    temp.Set(0, 0, 0, (ushort)MAVLink.MAV_CMD.DO_SET_CAM_TRIGG_DIST);
                    temp.p1 = (float)val_spacing;
                    temp.p2 = temp.p3 = temp.p4 = 0;
                    commands.Add(temp);
                }
                if (ascgrid[a].Tag == "ME")
                {
                    var temp = new Locationwp();
                    temp.Set(0, 0, 0, (ushort)MAVLink.MAV_CMD.DO_SET_CAM_TRIGG_DIST);
                    temp.p1 = temp.p2 = temp.p3 = temp.p4 = 0;
                    commands.Add(temp);
                }
            }

            return commands;
        }

        void saveWPs(object sender, ProgressWorkerEventArgs e, object passdata = null)
        {
            try
            {
                MAVLinkInterface port = ASCGrid.Host2.comPort;
                MainV2 mainform = ASCGrid.Host2.MainForm;

                if (!port.BaseStream.IsOpen)
                {
                    throw new Exception("Please connect first!");
                }

                MainV2.comPort.giveComport = true;
                int a = 0;

                if (home.lng == 0 && home.alt == 0 && home.lat == 0)
                {
                    MessageBox.Show("尚未设置HOME点", "错误");
                    return;
                }

                bool use_int = (port.MAV.cs.capabilities & MAVLink.MAV_PROTOCOL_CAPABILITY.MISSION_INT) > 0;

                // set wp total
                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(0, "Set total wps ");

                // get the command list from the datagrid
                var commandlist = GetCommandList();

                ushort totalwpcountforupload = (ushort)(commandlist.Count + 1);

                if (port.MAV.apname == MAVLink.MAV_AUTOPILOT.PX4)
                {
                    totalwpcountforupload--;
                }

                port.setWPTotal(totalwpcountforupload); // + home

                // set home location - overwritten/ignored depending on firmware.
                ((ProgressReporterDialogue)sender).UpdateProgressAndStatus(0, "设置家");

                // upload from wp0
                a = 0;

                if (port.MAV.apname != MAVLink.MAV_AUTOPILOT.PX4)
                {
                    try
                    {
                        var homeans = port.setWP(home, (ushort)a, MAVLink.MAV_FRAME.GLOBAL, 0, 1, use_int);
                        if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                        {
                            CustomMessageBox.Show(Strings.ErrorRejectedByMAV, Strings.ERROR);
                            return;
                        }
                        a++;
                    }
                    catch
                    {
                        use_int = false;
                        // added here to prevent timeout errors
                        port.setWPTotal(totalwpcountforupload);
                        var homeans = port.setWP(home, (ushort)a, MAVLink.MAV_FRAME.GLOBAL, 0, 1, use_int);
                        if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                        {
                            CustomMessageBox.Show(Strings.ErrorRejectedByMAV, Strings.ERROR);
                            return;
                        }
                        a++;
                    }
                }
                else
                {
                    use_int = false;
                }

                // define the default frame.
                MAVLink.MAV_FRAME frame = MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT;

                // process commandlist to the mav
                for (a = 1; a <= commandlist.Count; a++)
                {
                    var temp = commandlist[a-1];

                    ((ProgressReporterDialogue) sender).UpdateProgressAndStatus(a*100/commandlist.Count,
                        "上传" + a +"号航点中");

                    // make sure we are using the correct frame for these commands
                    if (temp.id < (ushort)MAVLink.MAV_CMD.LAST || temp.id == (ushort)MAVLink.MAV_CMD.DO_SET_HOME)
                    {
                        var mode = mainform.FlightPlanner.currentaltmode;

                        if (mode == FlightPlanner.altmode.Terrain)
                        {
                            frame = MAVLink.MAV_FRAME.GLOBAL_TERRAIN_ALT;
                        }
                        else if (mode == FlightPlanner.altmode.Absolute)
                        {
                            frame = MAVLink.MAV_FRAME.GLOBAL;
                        }
                        else
                        {
                            frame = MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT;
                        }
                    }

                    // try send the wp
                    MAVLink.MAV_MISSION_RESULT ans = port.setWP(temp, (ushort)(a), frame, 0, 1, use_int);

                    // we timed out while uploading wps/ command wasnt replaced/ command wasnt added
                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ERROR)
                    {
                        // resend for partial upload
                        port.setWPPartialUpdate((ushort) (a), totalwpcountforupload);
                        // reupload this point.
                        ans = port.setWP(temp, (ushort) (a), frame, 0, 1, use_int);
                    }

                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_NO_SPACE)
                    {
                        e.ErrorMessage = "上传失败，航点数过多";
                        return;
                    }
                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID)
                    {
                        e.ErrorMessage =
                            "上传失败, 任务被无人机拒绝,\n item had a bad option wp# " + a + " " +
                            ans;
                        return;
                    }
                    if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID_SEQUENCE)
                    {
                        // invalid sequence can only occur if we failed to see a response from the apm when we sent the request.
                        // or there is io lag and we send 2 mission_items and get 2 responces, one valid, one a ack of the second send
                        
                        // the ans is received via mission_ack, so we dont know for certain what our current request is for. as we may have lost the mission_request

                        // get requested wp no - 1;
                        a = port.getRequestedWPNo() - 1;

                        continue;
                    }
                    if (ans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                    {
                        e.ErrorMessage = "上传航点失败 " + Enum.Parse(typeof (MAVLink.MAV_CMD), temp.id.ToString()) +
                                         " " + Enum.Parse(typeof (MAVLink.MAV_MISSION_RESULT), ans.ToString());
                        return;
                    }
                }

                port.setWPACK();

                ((ProgressReporterDialogue) sender).UpdateProgressAndStatus(95, "设置参数中");

                // m
                port.setParam("WP_RADIUS", (float)NUM_wprad.Value/CurrentState.multiplierdist);

                // cm's
                port.setParam("WPNAV_RADIUS", (float)NUM_wprad.Value/CurrentState.multiplierdist*100.0);

                try
                {
                    port.setParam(new[] {"LOITER_RAD", "WP_LOITER_RAD"},
                        float.Parse(mainform.FlightPlanner.TXT_loiterrad.Text)/CurrentState.multiplierdist);
                }
                catch
                {
                }

                ((ProgressReporterDialogue) sender).UpdateProgressAndStatus(100, "已成功完成");
            }
            catch
            {
                MainV2.comPort.giveComport = false;
                throw;
            }

            MainV2.comPort.giveComport = false;
        }

        #region 地图控件相关

        void myGMAP1_OnCurrentPositionChanged(PointLatLng point)
        {
            //FlightPlanner.airportsoverlay.Clear();
            //foreach (var item in Airports.getAirports(myGMAP1.Position))
            //{
            //    FlightPlanner.airportsoverlay.Markers.Add(new GMapMarkerAirport(item)
            //    {
            //        ToolTipText = item.Tag,
            //        ToolTipMode = MarkerTooltipMode.OnMouseOver
            //    });
            //}
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

        private void myGMAP1_OnRouteEnter(GMapRoute item)
        {
            string dist;
            dist = ((float)item.Distance * 1000f).ToString("0.##") + " m";
            if (marker != null)
            {
                if (ascroutesOverlay.Markers.Contains(marker))
                    ascroutesOverlay.Markers.Remove(marker);
            }

            PointLatLng point = currentMousePosition;

            marker = new GMapMarkerRect(point);
            marker.ToolTip = new GMapToolTip(marker);
            marker.ToolTipMode = MarkerTooltipMode.Always;
            marker.ToolTipText = "Line: " + dist;
            ascroutesOverlay.Markers.Add(marker);
        }

        private void myGMAP1_OnRouteLeave(GMapRoute item)
        {
            if (marker != null)
            {
                try
                {
                    if (ascroutesOverlay.Markers.Contains(marker))
                        ascroutesOverlay.Markers.Remove(marker);
                }
                catch { }
            }
        }

        private void myGMAP1_OnMarkerEnter(GMapMarker item)
        {
            if (!isMouseDown)
            {
                if (item is GMapMarker)
                {
                    CurrentGMapMarker = item as GMapMarker;
                    CurrentGMapMarkerStartPos = CurrentGMapMarker.Position;
                }
            }
        }

        private void myGMAP1_OnMarkerLeave(GMapMarker item)
        {
            if (!isMouseDown)
            {
                if (item is GMapMarker)
                {
                    // when you click the context menu this triggers and causes problems
                    CurrentGMapMarker = null;
                }

            }
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
            ascgrid = new List<PointLatLngAlt>();
            
            ascgrid = Grid.CreateGrid(asclist, (double)NUM_altitude.Value,
                val_distance, val_spacing, (double)NUM_angle.Value,
                val_overshoot1, val_overshoot2,
                startpos, false,
                val_minLaneSeparation, val_leadin, val_adjust);

            myGMAP1.HoldInvalidation = true;
            
            if(ascroutesOverlay != null)
            {
                ascroutesOverlay.Markers.Clear();
                ascroutesOverlay.Routes.Clear();
                ascroutesOverlay.Polygons.Clear();
            }

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

                try
                {
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
                catch { }

            }
            try
            {
                myGMAP1.HoldInvalidation = false;
                if (!isMouseDown)
                    myGMAP1.ZoomAndCenterMarkers(ascroutesOverlay.Id);

                myGMAP1.Invalidate();
            }
            catch { }

        }

        private void Button_HOME_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.MAV.cs.lat != 0)
            {
                home.lat = MainV2.comPort.MAV.cs.lat;
                home.alt = MainV2.comPort.MAV.cs.altasl;
                home.lng = MainV2.comPort.MAV.cs.lng;
            }
            else
            {
                CustomMessageBox.Show(
                    "如果你在场地上，请连接你的飞控并等待GPS锁定，然后重试","未连接或无GPS");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            startpos = (Grid.StartPosition)Enum.Parse(typeof(Grid.StartPosition), comboBox1.Text);
            domainUpDown1_ValueChanged(sender,e);
        }

        private void WriteWP_Click(object sender, EventArgs e)
        {
            ProgressReporterDialogue frmProgressReporter = new ProgressReporterDialogue
            {
                StartPosition = FormStartPosition.CenterScreen,
                Text = "发送航点中"
            };

            frmProgressReporter.DoWork += saveWPs;
            frmProgressReporter.UpdateProgressAndStatus(-1, "发送航点中");

            ThemeManager.ApplyThemeTo(frmProgressReporter);

            frmProgressReporter.RunBackgroundOperationAsync();

            frmProgressReporter.Dispose();
        }

        private void ASCGridUI_Load(object sender, EventArgs e)
        {
            TRK_zoom.Value = (float)myGMAP1.Zoom;
            MainV2 mainform = ASCGrid.Host2.MainForm;
            try
            {
                home.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                home.lat = (double.Parse(mainform.FlightPlanner.TXT_homelat.Text));
                home.lng = (double.Parse(mainform.FlightPlanner.TXT_homelng.Text));
                home.alt = (float.Parse(mainform.FlightPlanner.TXT_homealt.Text) / CurrentState.multiplierdist); // use saved home
            }
            catch
            {
                MessageBox.Show("尚未设置HOME点，请在GPS锁定后点击设置HOME点","警告");
            }

            domainUpDown1_ValueChanged(this, null);//绘制航点
        }

        private void ASCGridUI_Resize(object sender, EventArgs e)
        {
            myGMAP1.ZoomAndCenterMarkers(ascroutesOverlay.Id);    //在改变窗口大小时，依旧保持zoom和中心在合适位置
        }

        public void CreateXml()
        {
            XmlDocument ascxmldoc = new XmlDocument();
            XmlNode ascnode = ascxmldoc.CreateXmlDeclaration("1.0", "utf-8", "");
            ascxmldoc.AppendChild(ascnode);
            XmlNode ascroot = ascxmldoc.CreateElement("pix4uav");
            ascxmldoc.AppendChild(ascroot);

            CreateNode(ascxmldoc, ascroot, "id", "non");
            CreateNode(ascxmldoc, ascroot, "version", "1.1.38");
            CreateNode(ascxmldoc, ascroot, "projectVersion", "1.1.38");
            CreateNode(ascxmldoc, ascroot, "cameraModelVersion", "4");
            CreateNode(ascxmldoc, ascroot, "userName", "non");
            CreateNode(ascxmldoc, ascroot, "WKT", "Local");
            CreateNode(ascxmldoc, ascroot, "WKTGCP", "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]");
            CreateNode(ascxmldoc, ascroot, "WKTImages", "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]");
            CreateNode(ascxmldoc, ascroot, "projectType", "aerial");
            CreateNode(ascxmldoc, ascroot, "uploaded", "no");
            CreateNode(ascxmldoc, ascroot, "processingType", "full");
            CreateNode(ascxmldoc, ascroot, "processingMethod", "standard");
            CreateNode(ascxmldoc, ascroot, "rigProcessing", "2");
            CreateNode(ascxmldoc, ascroot, "featureScale", "1");
            CreateNode(ascxmldoc, ascroot, "threshAdd", "20");
            CreateNode(ascxmldoc, ascroot, "thresHom", "10");
            CreateNode(ascxmldoc, ascroot, "gcpAccuracy", "10");
            CreateNode(ascxmldoc, ascroot, "minImageDist", "10");
            CreateNode(ascxmldoc, ascroot, "updateInternals", "1");
            CreateNode(ascxmldoc, ascroot, "relationOnMatch", "false");
            CreateNode(ascxmldoc, ascroot, "rematch", "true");
            CreateNode(ascxmldoc, ascroot, "minOrthoZoom", "0");
            CreateNode(ascxmldoc, ascroot, "maxOrthoZoom", "0");
            CreateNode(ascxmldoc, ascroot, "meanResolution", "0");
            CreateNode(ascxmldoc, ascroot, "orthoResolution", "-1");
            CreateNode(ascxmldoc, ascroot, "imageNormalisation", "false");
            CreateNode(ascxmldoc, ascroot, "denseSamplingDistance", "2");
            CreateNode(ascxmldoc, ascroot, "denseSamplingScale", "1");
            CreateNode(ascxmldoc, ascroot, "denseMinMatches", "3");
            CreateNode(ascxmldoc, ascroot, "denseMultiScale", "Multi");
            CreateNode(ascxmldoc, ascroot, "denseUseArea", "yes");
            CreateNode(ascxmldoc, ascroot, "denseUseAnnotations", "yes");
            CreateNode(ascxmldoc, ascroot, "denseUseNoiseFiltering", "yes");
            CreateNode(ascxmldoc, ascroot, "denseUseSurfaceSmoothing", "yes");
            CreateNode(ascxmldoc, ascroot, "denseFilterMedianRadius", "10");
            CreateNode(ascxmldoc, ascroot, "denseFilterSmoothRadius", "10");
            CreateNode(ascxmldoc, ascroot, "denseFilterSmoothLevel", "2");

            XmlElement cameraroot = ascxmldoc.CreateElement("camera");
            cameraroot.SetAttribute("cameraId", "EX-ZR100_6.6_4000x3000");
            cameraroot.SetAttribute("id", "");
            CreateNode(ascxmldoc, cameraroot, "imageWidth", "4000");
            CreateNode(ascxmldoc, cameraroot, "imageHeight", "3000");
            CreateNode(ascxmldoc, cameraroot, "pixelSize", "1.56318");
            CreateNode(ascxmldoc, cameraroot, "principalPointXmm", "3.12635");
            CreateNode(ascxmldoc, cameraroot, "principalPointYmm", "2.34476");
            CreateNode(ascxmldoc, cameraroot, "pixelType", "3");
            CreateNode2(ascxmldoc, cameraroot, "pixelValue", "min", "0", "max", "0");
            CreateNode3(ascxmldoc, cameraroot, "band", "id", "1", "name", "Red", "weight", "0.2126");
            CreateNode3(ascxmldoc, cameraroot, "band", "id", "2", "name", "Green", "weight", "0.7152");
            CreateNode3(ascxmldoc, cameraroot, "band", "id", "3", "name", "Blue", "weight", "0.0722");
            CreateNode(ascxmldoc, cameraroot, "cameraType", "perspective");
            CreateNode(ascxmldoc, cameraroot, "focalLengthmm", "6.61");
            CreateNode(ascxmldoc, cameraroot, "radialK1", "0");
            CreateNode(ascxmldoc, cameraroot, "radialK2", "0");
            CreateNode(ascxmldoc, cameraroot, "radialK3", "0");
            CreateNode(ascxmldoc, cameraroot, "tangentialT1", "0");
            CreateNode(ascxmldoc, cameraroot, "tangentialT2", "0");
            CreateNode(ascxmldoc, cameraroot, "source", "exif");
            CreateNode(ascxmldoc, cameraroot, "sensorWidthmm", "6.2527");
            CreateNode(ascxmldoc, cameraroot, "index", "0");
            CreateNode(ascxmldoc, cameraroot, "fixedBands", "false");

            ascroot.AppendChild(cameraroot);

            foreach (var item in imageinfo)
            {
                XmlElement imageroot = ascxmldoc.CreateElement("image");
                imageroot.SetAttribute("path", item.ImagePath);
                imageroot.SetAttribute("type", item.type);
                imageroot.SetAttribute("enabled", item.enabled);

                item.CreateImageNode2(ascxmldoc, imageroot, "camera", "id", item.cameraid, "index", item.cameraindex);
                item.CreateImageNode1(ascxmldoc, imageroot, "exifID", "value", item.exifid);
                item.CreateImageNode1(ascxmldoc, imageroot, "time", "value", item.time);
                item.CreateImageNode1(ascxmldoc, imageroot, "time", "value", item.timedouble);
                item.CreateImageNode3(ascxmldoc, imageroot, "gps", "lat", item.lat, "lng", item.lng, "alt", item.alt);
                item.CreateImageNode3(ascxmldoc, imageroot, "xyz", "x", item.x, "y", item.y, "z", item.z);
                item.CreateImageNode1(ascxmldoc, imageroot, "toleranceXY", "value", item.toleranceXY);
                item.CreateImageNode1(ascxmldoc, imageroot, "toleranceZ", "value", item.toleranceZ);

                ascroot.AppendChild(imageroot);
            }

            //foreach (var item in picturesInfo)
            //{
            //    XmlElement imageroot = ascxmldoc.CreateElement("image");
            //    imageroot.SetAttribute("path", item.Key);
            //    imageroot.SetAttribute("type", "group1");
            //    imageroot.SetAttribute("enabled", "true");

            //    CreateNode2(ascxmldoc, imageroot, "camera", "id", "EX-ZR100_6.6_4000x3000", "index", "0");
            //    CreateNode(ascxmldoc, imageroot, "exifID", "EX-ZR100_6.6_4000x3000");
            //    CreateNode(ascxmldoc, imageroot, "time", item.Value.Time.ToString());
            //    CreateNode(ascxmldoc, imageroot, "time", "value", item.timedouble);
            //    CreateNode3(ascxmldoc, imageroot, "gps", "lat", item.Value.Lat.ToString(), "lng", item.Value.Lon.ToString(), "alt", item.Value.AltAMSL.ToString());
            //    CreateNode3(ascxmldoc, imageroot, "xyz", "x", item.x, "y", item.y, "z", item.z);
            //    CreateNode(ascxmldoc, imageroot, "toleranceXY", "5");
            //    CreateNode(ascxmldoc, imageroot, "toleranceZ", "10");

            //    ascroot.AppendChild(imageroot);
            //}

            ascxmldoc.Save(@"c:\Users\hasee\Desktop\test.xml");
        }

        private void CreateNode(XmlDocument xmldoc, XmlNode ParentNode, string name, string value)
        {
            XmlElement node = xmldoc.CreateElement(name);
            node.SetAttribute("value", value);
            ParentNode.AppendChild(node);
        }

        private void CreateNode2(XmlDocument xmldoc, XmlNode ParentNode, string nodename, string Name, string Value, string Name1, string Value1)
        {
            XmlElement node = xmldoc.CreateElement(nodename);
            node.SetAttribute(Name, Value);
            node.SetAttribute(Name1, Value1);
            ParentNode.AppendChild(node);
        }

        private void CreateNode3(XmlDocument xmldoc, XmlNode ParentNode, string nodename, string Name, string Value, string Name1, string Value1, string Name2, string Value2)
        {
            XmlElement node = xmldoc.CreateElement(nodename);
            node.SetAttribute(Name, Value);
            node.SetAttribute(Name1, Value1);
            node.SetAttribute(Name2, Value2);
            ParentNode.AppendChild(node);
        }

        #endregion

        #region 常数

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


        #endregion

        private void myButton1_Click(object sender, EventArgs e)
        {
            var form = new LogDownloadMavLink();
            form.Show();
        }

        private void BUT_browselog_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Logs|*.log;*.tlog;*.bin";
            openFileDialog1.ShowDialog();

            if (File.Exists(openFileDialog1.FileName))
            {
                TXT_logfile.Text = openFileDialog1.FileName;
                // TXT_jpgdir.Text = Path.GetDirectoryName(TXT_logfile.Text);
            }
        }

        private void BUT_browsedir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath != "")
            {
                TXT_jpgdir.Text = folderBrowserDialog1.SelectedPath;

            }
        }

        private void BUT_CreateP4D_Click(object sender, EventArgs e)
        {

            CreateXml();

            try
            {
                string strKeyName = "installDir";
                string softPath = @"SOFTWARE\pix4d\Pix4D mapper";
                RegistryKey regKey = Registry.CurrentUser;
                RegistryKey regSubKey = regKey.OpenSubKey(softPath, false);

                object objResult = regSubKey.GetValue(strKeyName);
                RegistryValueKind regValueKind = regSubKey.GetValueKind(strKeyName);

                if(objResult == null || objResult.ToString() == "")
                {
                    throw new Exception("File no exist.");
                }

                if (regValueKind == Microsoft.Win32.RegistryValueKind.String)
                {
                    Process p = Process.Start(objResult.ToString() + "pix4dmapper.exe");
                }
            }
            catch
            {
                openFileDialog1.Filter = "Pix4Dmapper|pix4dmapper.exe";
                openFileDialog1.ShowDialog();

                if (File.Exists(openFileDialog1.FileName))
                {
                    Process p = Process.Start(openFileDialog1.FileName);
                    // TXT_jpgdir.Text = Path.GetDirectoryName(TXT_logfile.Text);
                }
            }

        }

        private void BUT_Anal_Click(object sender, EventArgs e)
        {

        }
    }

    public class ImageInfo
    {
        public ImageInfo()
        {
            type = "group1";
            enabled = "true";
            cameraindex = "0";
        }

        public string ImagePath;
        public string type;
        public string enabled;
        public string cameraid;
        public string cameraindex;
        public string exifid;
        public string time;
        public string timedouble;

        public string lat, lng, alt;
        public string x, y, z;
        public string toleranceXY, toleranceZ;

        public void CreateImageNode3(XmlDocument xmldoc, XmlNode ParentNode, string nodename, string Name, string Value, string Name1, string Value1, string Name2, string Value2)
        {
            XmlElement node = xmldoc.CreateElement(nodename);
            node.SetAttribute(Name, Value);
            node.SetAttribute(Name1, Value1);
            node.SetAttribute(Name2, Value2);
            ParentNode.AppendChild(node);
        }

        public void CreateImageNode2(XmlDocument xmldoc, XmlNode ParentNode, string nodename, string Name, string Value, string Name1, string Value1)
        {
            XmlElement node = xmldoc.CreateElement(nodename);
            node.SetAttribute(Name, Value);
            node.SetAttribute(Name1, Value1);
            ParentNode.AppendChild(node);
        }

        public void CreateImageNode1(XmlDocument xmldoc, XmlNode ParentNode, string nodename, string Name, string Value)
        {
            XmlElement node = xmldoc.CreateElement(nodename);
            node.SetAttribute(Name, Value);
            ParentNode.AppendChild(node);
        }
    }
}
