namespace RTCVision
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.HWindowsMain = new HalconDotNet.HSmartWindowControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.cbModel = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.btnVisionSettings = new DevExpress.XtraEditors.SimpleButton();
            this.btnModelManager = new DevExpress.XtraEditors.SimpleButton();
            this.btnStop = new DevExpress.XtraEditors.SimpleButton();
            this.btnStart = new DevExpress.XtraEditors.SimpleButton();
            this.lblStatus = new DevExpress.XtraEditors.LabelControl();
            this.btnStartPLC = new DevExpress.XtraEditors.SimpleButton();
            this.btnStopPLC = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbModel.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // HWindowsMain
            // 
            this.HWindowsMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.HWindowsMain.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.HWindowsMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HWindowsMain.HDoubleClickToFitContent = true;
            this.HWindowsMain.HDrawingObjectsModifier = HalconDotNet.HSmartWindowControl.DrawingObjectsModifier.None;
            this.HWindowsMain.HImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.HWindowsMain.HKeepAspectRatio = true;
            this.HWindowsMain.HMoveContent = true;
            this.HWindowsMain.HZoomContent = HalconDotNet.HSmartWindowControl.ZoomContent.WheelForwardZoomsIn;
            this.HWindowsMain.Location = new System.Drawing.Point(0, 0);
            this.HWindowsMain.Margin = new System.Windows.Forms.Padding(0);
            this.HWindowsMain.Name = "HWindowsMain";
            this.HWindowsMain.Size = new System.Drawing.Size(1619, 754);
            this.HWindowsMain.TabIndex = 0;
            this.HWindowsMain.WindowSize = new System.Drawing.Size(1619, 754);
            this.HWindowsMain.Load += new System.EventHandler(this.FrmMain_Load);
            // 
            // panelControl1
            // 
            this.panelControl1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelControl1.Appearance.Options.UseBackColor = true;
            this.panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl1.Controls.Add(this.cbModel);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.btnClose);
            this.panelControl1.Controls.Add(this.btnVisionSettings);
            this.panelControl1.Controls.Add(this.btnModelManager);
            this.panelControl1.Controls.Add(this.btnStopPLC);
            this.panelControl1.Controls.Add(this.btnStop);
            this.panelControl1.Controls.Add(this.btnStartPLC);
            this.panelControl1.Controls.Add(this.btnStart);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.LookAndFeel.SkinName = "DevExpress Style";
            this.panelControl1.LookAndFeel.UseDefaultLookAndFeel = false;
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panelControl1.Size = new System.Drawing.Size(1619, 110);
            this.panelControl1.TabIndex = 1;
            // 
            // cbModel
            // 
            this.cbModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbModel.Location = new System.Drawing.Point(860, 52);
            this.cbModel.Name = "cbModel";
            this.cbModel.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.Appearance.Options.UseFont = true;
            this.cbModel.Properties.AppearanceDisabled.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceDisabled.Options.UseFont = true;
            this.cbModel.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceDropDown.Options.UseFont = true;
            this.cbModel.Properties.AppearanceFocused.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceFocused.Options.UseFont = true;
            this.cbModel.Properties.AppearanceItemDisabled.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceItemDisabled.Options.UseFont = true;
            this.cbModel.Properties.AppearanceItemHighlight.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceItemHighlight.Options.UseFont = true;
            this.cbModel.Properties.AppearanceItemSelected.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceItemSelected.Options.UseFont = true;
            this.cbModel.Properties.AppearanceReadOnly.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceReadOnly.Options.UseFont = true;
            this.cbModel.Properties.AutoHeight = false;
            this.cbModel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbModel.Size = new System.Drawing.Size(252, 34);
            this.cbModel.TabIndex = 9;
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.ForeColor = System.Drawing.Color.White;
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Appearance.Options.UseForeColor = true;
            this.labelControl1.Location = new System.Drawing.Point(860, 13);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(119, 28);
            this.labelControl1.TabIndex = 7;
            this.labelControl1.Text = "Model List";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Appearance.Options.UseFont = true;
            this.btnClose.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnClose.ImageOptions.SvgImage")));
            this.btnClose.Location = new System.Drawing.Point(1463, 33);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(129, 53);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Exit";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnVisionSettings
            // 
            this.btnVisionSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVisionSettings.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVisionSettings.Appearance.Options.UseFont = true;
            this.btnVisionSettings.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnVisionSettings.ImageOptions.SvgImage")));
            this.btnVisionSettings.Location = new System.Drawing.Point(1297, 33);
            this.btnVisionSettings.Name = "btnVisionSettings";
            this.btnVisionSettings.Size = new System.Drawing.Size(138, 53);
            this.btnVisionSettings.TabIndex = 3;
            this.btnVisionSettings.Text = "Setting";
            this.btnVisionSettings.Click += new System.EventHandler(this.btnVisionSettings_Click);
            // 
            // btnModelManager
            // 
            this.btnModelManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModelManager.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModelManager.Appearance.Options.UseFont = true;
            this.btnModelManager.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnModelManager.ImageOptions.SvgImage")));
            this.btnModelManager.Location = new System.Drawing.Point(1145, 33);
            this.btnModelManager.Name = "btnModelManager";
            this.btnModelManager.Size = new System.Drawing.Size(129, 53);
            this.btnModelManager.TabIndex = 2;
            this.btnModelManager.Text = "Model";
            // 
            // btnStop
            // 
            this.btnStop.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Appearance.Options.UseFont = true;
            this.btnStop.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnStop.ImageOptions.SvgImage")));
            this.btnStop.Location = new System.Drawing.Point(205, 33);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(129, 53);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Appearance.Options.UseFont = true;
            this.btnStart.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnStart.ImageOptions.SvgImage")));
            this.btnStart.Location = new System.Drawing.Point(35, 33);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(129, 53);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblStatus.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Appearance.Options.UseBackColor = true;
            this.lblStatus.Appearance.Options.UseFont = true;
            this.lblStatus.Appearance.Options.UseForeColor = true;
            this.lblStatus.Appearance.Options.UseTextOptions = true;
            this.lblStatus.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblStatus.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStatus.Location = new System.Drawing.Point(1463, 116);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(144, 67);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "OK";
            this.lblStatus.Visible = false;
            // 
            // btnStartPLC
            // 
            this.btnStartPLC.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartPLC.Appearance.Options.UseFont = true;
            this.btnStartPLC.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton1.ImageOptions.SvgImage")));
            this.btnStartPLC.Location = new System.Drawing.Point(416, 33);
            this.btnStartPLC.Name = "btnStartPLC";
            this.btnStartPLC.Size = new System.Drawing.Size(168, 53);
            this.btnStartPLC.TabIndex = 0;
            this.btnStartPLC.Text = "Start PLC";
            this.btnStartPLC.Click += new System.EventHandler(this.btnStartPLC_Click);
            // 
            // btnStopPLC
            // 
            this.btnStopPLC.Appearance.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopPLC.Appearance.Options.UseFont = true;
            this.btnStopPLC.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton2.ImageOptions.SvgImage")));
            this.btnStopPLC.Location = new System.Drawing.Point(641, 33);
            this.btnStopPLC.Name = "btnStopPLC";
            this.btnStopPLC.Size = new System.Drawing.Size(169, 53);
            this.btnStopPLC.TabIndex = 1;
            this.btnStopPLC.Text = "Stop PLC";
            this.btnStopPLC.Click += new System.EventHandler(this.btnStopPLC_Click);
            // 
            // FrmMain
            // 
            this.Appearance.BackColor = System.Drawing.Color.White;
            this.Appearance.Options.UseBackColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1619, 754);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.HWindowsMain);
            this.Name = "FrmMain";
            this.Text = "Vision";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbModel.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private HalconDotNet.HSmartWindowControl HWindowsMain;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnVisionSettings;
        private DevExpress.XtraEditors.SimpleButton btnModelManager;
        private DevExpress.XtraEditors.SimpleButton btnStop;
        private DevExpress.XtraEditors.SimpleButton btnStart;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl lblStatus;
        private DevExpress.XtraEditors.ComboBoxEdit cbModel;
        private DevExpress.XtraEditors.SimpleButton btnStopPLC;
        private DevExpress.XtraEditors.SimpleButton btnStartPLC;
    }
}