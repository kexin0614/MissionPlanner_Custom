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
using System.Collections;

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
        private double val_distance;
        private double val_spacing;
        private double val_overshoot1;
        private double val_overshoot2;
        private float val_minLaneSeparation;
        private float val_leadin;
        private double val_adjust;
        private Grid.StartPosition startpos;

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


    }
}
