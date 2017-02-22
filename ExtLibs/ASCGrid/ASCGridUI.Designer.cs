namespace MissionPlanner
{
    partial class ASCGridUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.myGMAP1 = new MissionPlanner.Controls.myGMAP();
            this.TRK_zoom = new MissionPlanner.Controls.MyTrackBar();
            this.WriteWP = new MissionPlanner.Controls.MyButton();
            this.label5 = new System.Windows.Forms.Label();
            this.NUM_altitude = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.NUM_turnparam = new System.Windows.Forms.NumericUpDown();
            this.num_sidelap = new System.Windows.Forms.NumericUpDown();
            this.num_overlap = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.NUM_angle = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox_MAV = new System.Windows.Forms.GroupBox();
            this.Button_HOME = new MissionPlanner.Controls.MyButton();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.NUM_wprad = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TRK_zoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_altitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_turnparam)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_sidelap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_overlap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_angle)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox_MAV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_wprad)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 69.04501F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3.951701F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.0033F));
            this.tableLayoutPanel1.Controls.Add(this.myGMAP1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.TRK_zoom, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox_MAV, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(-1, -3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 255F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(910, 532);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // myGMAP1
            // 
            this.myGMAP1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.myGMAP1.Bearing = 0F;
            this.myGMAP1.CanDragMap = true;
            this.myGMAP1.EmptyTileColor = System.Drawing.Color.Navy;
            this.myGMAP1.GrayScaleMode = false;
            this.myGMAP1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.myGMAP1.LevelsKeepInMemmory = 5;
            this.myGMAP1.Location = new System.Drawing.Point(3, 3);
            this.myGMAP1.MarkersEnabled = true;
            this.myGMAP1.MaxZoom = 19;
            this.myGMAP1.MinZoom = 0;
            this.myGMAP1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.myGMAP1.Name = "myGMAP1";
            this.myGMAP1.NegativeMode = false;
            this.myGMAP1.PolygonsEnabled = true;
            this.myGMAP1.RetryLoadTile = 0;
            this.myGMAP1.RoutesEnabled = true;
            this.tableLayoutPanel1.SetRowSpan(this.myGMAP1, 3);
            this.myGMAP1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional;
            this.myGMAP1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.myGMAP1.ShowTileGridLines = false;
            this.myGMAP1.Size = new System.Drawing.Size(622, 526);
            this.myGMAP1.TabIndex = 0;
            this.myGMAP1.Zoom = 16D;
            this.myGMAP1.OnRouteEnter += new GMap.NET.WindowsForms.RouteEnter(this.myGMAP1_OnRouteEnter);
            this.myGMAP1.OnRouteLeave += new GMap.NET.WindowsForms.RouteLeave(this.myGMAP1_OnRouteLeave);
            this.myGMAP1.OnMarkerEnter += new GMap.NET.WindowsForms.MarkerEnter(this.myGMAP1_OnMarkerEnter);
            this.myGMAP1.OnMarkerLeave += new GMap.NET.WindowsForms.MarkerLeave(this.myGMAP1_OnMarkerLeave);
            this.myGMAP1.OnPositionChanged += new GMap.NET.PositionChanged(this.myGMAP1_OnCurrentPositionChanged);
            this.myGMAP1.OnMapZoomChanged += new GMap.NET.MapZoomChanged(this.myGMAP1_OnMapZoomChanged);
            this.myGMAP1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.myGMAP1_MouseDown);
            this.myGMAP1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.myGMAP1_MouseMove);
            this.myGMAP1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.myGMAP1_MouseUp);
            // 
            // TRK_zoom
            // 
            this.TRK_zoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TRK_zoom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.TRK_zoom.LargeChange = 0.005F;
            this.TRK_zoom.Location = new System.Drawing.Point(631, 3);
            this.TRK_zoom.Maximum = 19F;
            this.TRK_zoom.Minimum = 2F;
            this.TRK_zoom.Name = "TRK_zoom";
            this.TRK_zoom.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tableLayoutPanel1.SetRowSpan(this.TRK_zoom, 3);
            this.TRK_zoom.Size = new System.Drawing.Size(29, 526);
            this.TRK_zoom.SmallChange = 0.001F;
            this.TRK_zoom.TabIndex = 48;
            this.TRK_zoom.TickFrequency = 1F;
            this.TRK_zoom.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.TRK_zoom.Value = 2F;
            this.TRK_zoom.Scroll += new System.EventHandler(this.TRK_zoom_Scroll);
            // 
            // WriteWP
            // 
            this.WriteWP.Location = new System.Drawing.Point(129, 20);
            this.WriteWP.Name = "WriteWP";
            this.WriteWP.Size = new System.Drawing.Size(75, 23);
            this.WriteWP.TabIndex = 49;
            this.WriteWP.Text = "写入航点";
            this.WriteWP.UseVisualStyleBackColor = true;
            this.WriteWP.Click += new System.EventHandler(this.WriteWP_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "飞行高度";
            // 
            // NUM_altitude
            // 
            this.NUM_altitude.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.NUM_altitude.Location = new System.Drawing.Point(129, 20);
            this.NUM_altitude.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.NUM_altitude.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUM_altitude.Name = "NUM_altitude";
            this.NUM_altitude.Size = new System.Drawing.Size(81, 21);
            this.NUM_altitude.TabIndex = 2;
            this.NUM_altitude.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.NUM_altitude.ValueChanged += new System.EventHandler(this.domainUpDown1_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 4;
            this.label6.Text = "转弯参数";
            // 
            // NUM_turnparam
            // 
            this.NUM_turnparam.Location = new System.Drawing.Point(129, 47);
            this.NUM_turnparam.Name = "NUM_turnparam";
            this.NUM_turnparam.Size = new System.Drawing.Size(81, 21);
            this.NUM_turnparam.TabIndex = 5;
            this.NUM_turnparam.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.NUM_turnparam.ValueChanged += new System.EventHandler(this.domainUpDown1_ValueChanged);
            // 
            // num_sidelap
            // 
            this.num_sidelap.DecimalPlaces = 1;
            this.num_sidelap.Location = new System.Drawing.Point(129, 101);
            this.num_sidelap.Name = "num_sidelap";
            this.num_sidelap.Size = new System.Drawing.Size(81, 21);
            this.num_sidelap.TabIndex = 63;
            this.num_sidelap.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.num_sidelap.ValueChanged += new System.EventHandler(this.domainUpDown1_ValueChanged);
            // 
            // num_overlap
            // 
            this.num_overlap.DecimalPlaces = 1;
            this.num_overlap.Location = new System.Drawing.Point(129, 74);
            this.num_overlap.Name = "num_overlap";
            this.num_overlap.Size = new System.Drawing.Size(81, 21);
            this.num_overlap.TabIndex = 62;
            this.num_overlap.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.num_overlap.ValueChanged += new System.EventHandler(this.domainUpDown1_ValueChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(6, 103);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(89, 12);
            this.label15.TabIndex = 61;
            this.label15.Text = "旁向重叠度 [%]";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(6, 76);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 12);
            this.label8.TabIndex = 60;
            this.label8.Text = "航向重叠度 [%]";
            // 
            // NUM_angle
            // 
            this.NUM_angle.Location = new System.Drawing.Point(129, 128);
            this.NUM_angle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.NUM_angle.Name = "NUM_angle";
            this.NUM_angle.Size = new System.Drawing.Size(81, 21);
            this.NUM_angle.TabIndex = 64;
            this.NUM_angle.ValueChanged += new System.EventHandler(this.domainUpDown1_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(6, 130);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 12);
            this.label7.TabIndex = 65;
            this.label7.Text = "扫描航向 [°]";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.NUM_wprad);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.NUM_angle);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.num_overlap);
            this.groupBox2.Controls.Add(this.num_sidelap);
            this.groupBox2.Controls.Add(this.NUM_turnparam);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.NUM_altitude);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(666, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(228, 210);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "航线设置";
            // 
            // groupBox_MAV
            // 
            this.groupBox_MAV.Controls.Add(this.Button_HOME);
            this.groupBox_MAV.Controls.Add(this.WriteWP);
            this.groupBox_MAV.Location = new System.Drawing.Point(666, 219);
            this.groupBox_MAV.Name = "groupBox_MAV";
            this.groupBox_MAV.Size = new System.Drawing.Size(241, 52);
            this.groupBox_MAV.TabIndex = 50;
            this.groupBox_MAV.TabStop = false;
            this.groupBox_MAV.Text = "任务";
            // 
            // Button_HOME
            // 
            this.Button_HOME.Location = new System.Drawing.Point(37, 20);
            this.Button_HOME.Name = "Button_HOME";
            this.Button_HOME.Size = new System.Drawing.Size(75, 23);
            this.Button_HOME.TabIndex = 50;
            this.Button_HOME.Text = "设置HOME点";
            this.Button_HOME.UseVisualStyleBackColor = true;
            this.Button_HOME.Click += new System.EventHandler(this.Button_HOME_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(6, 185);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 66;
            this.label1.Text = "起始位置";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(129, 182);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(81, 20);
            this.comboBox1.TabIndex = 67;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // NUM_wprad
            // 
            this.NUM_wprad.Location = new System.Drawing.Point(129, 155);
            this.NUM_wprad.Name = "NUM_wprad";
            this.NUM_wprad.Size = new System.Drawing.Size(81, 21);
            this.NUM_wprad.TabIndex = 68;
            this.NUM_wprad.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.NUM_wprad.ValueChanged += new System.EventHandler(this.domainUpDown1_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(6, 157);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 69;
            this.label2.Text = "航点半径 [m]";
            // 
            // ASCGridUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 525);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ASCGridUI";
            this.Text = "ASCGridUI";
            this.Load += new System.EventHandler(this.ASCGridUI_Load);
            this.Resize += new System.EventHandler(this.ASCGridUI_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TRK_zoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_altitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_turnparam)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_sidelap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_overlap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUM_angle)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox_MAV.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NUM_wprad)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.myGMAP myGMAP1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Controls.MyTrackBar TRK_zoom;
        private Controls.MyButton WriteWP;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown NUM_angle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown num_overlap;
        private System.Windows.Forms.NumericUpDown num_sidelap;
        private System.Windows.Forms.NumericUpDown NUM_turnparam;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown NUM_altitude;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox_MAV;
        private Controls.MyButton Button_HOME;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown NUM_wprad;
    }
}