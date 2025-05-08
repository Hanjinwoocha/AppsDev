namespace EnrollmentSystem
{
    partial class Sidebar
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
            this.components = new System.ComponentModel.Container();
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.TopControl = new Guna.UI2.WinForms.Guna2Panel();
            this.guna2ControlBox1 = new Guna.UI2.WinForms.Guna2ControlBox();
            this.SideBarTabControl = new Guna.UI2.WinForms.Guna2TabControl();
            this.TabPageHome = new System.Windows.Forms.TabPage();
            this.TabPageStudentEntry = new System.Windows.Forms.TabPage();
            this.TabPageSubjectEntry = new System.Windows.Forms.TabPage();
            this.TabPageScheduleEntry = new System.Windows.Forms.TabPage();
            this.TabPageEnrollmentEntry = new System.Windows.Forms.TabPage();
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.TabPageStudentGrade = new System.Windows.Forms.TabPage();
            this.TopControl.SuspendLayout();
            this.SideBarTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.TargetControl = this;
            // 
            // TopControl
            // 
            this.TopControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(42)))), ((int)(((byte)(57)))));
            this.TopControl.Controls.Add(this.guna2ControlBox1);
            this.TopControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopControl.Location = new System.Drawing.Point(0, 0);
            this.TopControl.Name = "TopControl";
            this.TopControl.Size = new System.Drawing.Size(1000, 48);
            this.TopControl.TabIndex = 8;
            // 
            // guna2ControlBox1
            // 
            this.guna2ControlBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.guna2ControlBox1.FillColor = System.Drawing.Color.Transparent;
            this.guna2ControlBox1.IconColor = System.Drawing.Color.White;
            this.guna2ControlBox1.Location = new System.Drawing.Point(958, 12);
            this.guna2ControlBox1.Name = "guna2ControlBox1";
            this.guna2ControlBox1.Size = new System.Drawing.Size(30, 30);
            this.guna2ControlBox1.TabIndex = 0;
            // 
            // SideBarTabControl
            // 
            this.SideBarTabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.SideBarTabControl.Controls.Add(this.TabPageHome);
            this.SideBarTabControl.Controls.Add(this.TabPageStudentEntry);
            this.SideBarTabControl.Controls.Add(this.TabPageSubjectEntry);
            this.SideBarTabControl.Controls.Add(this.TabPageScheduleEntry);
            this.SideBarTabControl.Controls.Add(this.TabPageEnrollmentEntry);
            this.SideBarTabControl.Controls.Add(this.TabPageStudentGrade);
            this.SideBarTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SideBarTabControl.ItemSize = new System.Drawing.Size(180, 40);
            this.SideBarTabControl.Location = new System.Drawing.Point(0, 48);
            this.SideBarTabControl.Name = "SideBarTabControl";
            this.SideBarTabControl.SelectedIndex = 0;
            this.SideBarTabControl.Size = new System.Drawing.Size(1000, 652);
            this.SideBarTabControl.TabButtonHoverState.BorderColor = System.Drawing.Color.Empty;
            this.SideBarTabControl.TabButtonHoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(52)))), ((int)(((byte)(70)))));
            this.SideBarTabControl.TabButtonHoverState.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.SideBarTabControl.TabButtonHoverState.ForeColor = System.Drawing.Color.White;
            this.SideBarTabControl.TabButtonHoverState.InnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(52)))), ((int)(((byte)(70)))));
            this.SideBarTabControl.TabButtonIdleState.BorderColor = System.Drawing.Color.Empty;
            this.SideBarTabControl.TabButtonIdleState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(42)))), ((int)(((byte)(57)))));
            this.SideBarTabControl.TabButtonIdleState.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.SideBarTabControl.TabButtonIdleState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(160)))), ((int)(((byte)(167)))));
            this.SideBarTabControl.TabButtonIdleState.InnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(42)))), ((int)(((byte)(57)))));
            this.SideBarTabControl.TabButtonSelectedState.BorderColor = System.Drawing.Color.Empty;
            this.SideBarTabControl.TabButtonSelectedState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(37)))), ((int)(((byte)(49)))));
            this.SideBarTabControl.TabButtonSelectedState.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.SideBarTabControl.TabButtonSelectedState.ForeColor = System.Drawing.Color.White;
            this.SideBarTabControl.TabButtonSelectedState.InnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(132)))), ((int)(((byte)(255)))));
            this.SideBarTabControl.TabButtonSize = new System.Drawing.Size(180, 40);
            this.SideBarTabControl.TabIndex = 9;
            this.SideBarTabControl.TabMenuBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(42)))), ((int)(((byte)(57)))));
            this.SideBarTabControl.SelectedIndexChanged += new System.EventHandler(this.SIdeBarTabControl_SelectedIndexChanged);
            // 
            // TabPageHome
            // 
            this.TabPageHome.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TabPageHome.Location = new System.Drawing.Point(184, 4);
            this.TabPageHome.Name = "TabPageHome";
            this.TabPageHome.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageHome.Size = new System.Drawing.Size(812, 644);
            this.TabPageHome.TabIndex = 0;
            this.TabPageHome.Text = "Home";
            // 
            // TabPageStudentEntry
            // 
            this.TabPageStudentEntry.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TabPageStudentEntry.Location = new System.Drawing.Point(184, 4);
            this.TabPageStudentEntry.Name = "TabPageStudentEntry";
            this.TabPageStudentEntry.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageStudentEntry.Size = new System.Drawing.Size(812, 644);
            this.TabPageStudentEntry.TabIndex = 1;
            this.TabPageStudentEntry.Text = "Student Entry";
            // 
            // TabPageSubjectEntry
            // 
            this.TabPageSubjectEntry.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TabPageSubjectEntry.Location = new System.Drawing.Point(184, 4);
            this.TabPageSubjectEntry.Name = "TabPageSubjectEntry";
            this.TabPageSubjectEntry.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSubjectEntry.Size = new System.Drawing.Size(812, 644);
            this.TabPageSubjectEntry.TabIndex = 2;
            this.TabPageSubjectEntry.Text = "Subject Entry";
            // 
            // TabPageScheduleEntry
            // 
            this.TabPageScheduleEntry.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TabPageScheduleEntry.Location = new System.Drawing.Point(184, 4);
            this.TabPageScheduleEntry.Name = "TabPageScheduleEntry";
            this.TabPageScheduleEntry.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageScheduleEntry.Size = new System.Drawing.Size(812, 644);
            this.TabPageScheduleEntry.TabIndex = 3;
            this.TabPageScheduleEntry.Text = "Schedule Entry";
            // 
            // TabPageEnrollmentEntry
            // 
            this.TabPageEnrollmentEntry.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TabPageEnrollmentEntry.Location = new System.Drawing.Point(184, 4);
            this.TabPageEnrollmentEntry.Name = "TabPageEnrollmentEntry";
            this.TabPageEnrollmentEntry.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageEnrollmentEntry.Size = new System.Drawing.Size(812, 644);
            this.TabPageEnrollmentEntry.TabIndex = 4;
            this.TabPageEnrollmentEntry.Text = "Enrollment Entry";
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.TargetControl = this.TopControl;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // TabPageStudentGrade
            // 
            this.TabPageStudentGrade.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TabPageStudentGrade.Location = new System.Drawing.Point(184, 4);
            this.TabPageStudentGrade.Name = "TabPageStudentGrade";
            this.TabPageStudentGrade.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageStudentGrade.Size = new System.Drawing.Size(812, 644);
            this.TabPageStudentGrade.TabIndex = 5;
            this.TabPageStudentGrade.Text = "Student Grade";
            // 
            // Sidebar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.SideBarTabControl);
            this.Controls.Add(this.TopControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Sidebar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enrollment";
            this.TopControl.ResumeLayout(false);
            this.SideBarTabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private Guna.UI2.WinForms.Guna2TabControl SideBarTabControl;
        private System.Windows.Forms.TabPage TabPageHome;
        private System.Windows.Forms.TabPage TabPageStudentEntry;
        private System.Windows.Forms.TabPage TabPageSubjectEntry;
        private System.Windows.Forms.TabPage TabPageScheduleEntry;
        private Guna.UI2.WinForms.Guna2Panel TopControl;
        private Guna.UI2.WinForms.Guna2ControlBox guna2ControlBox1;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl1;
        private System.Windows.Forms.TabPage TabPageEnrollmentEntry;
        private System.Windows.Forms.TabPage TabPageStudentGrade;
    }
}

