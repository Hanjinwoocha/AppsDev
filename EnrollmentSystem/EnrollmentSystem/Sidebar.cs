using EnrollmentSystem.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnrollmentSystem
{
    public partial class Sidebar : Form
    {
        public Sidebar()
        {
            InitializeComponent();
        }

        public void ShowUserControl(UserControl control, TabPage tabPage)
        {
            if (!tabPage.Controls.Contains(control))
            {
                control.Dock = DockStyle.Fill;
                tabPage.Controls.Add(control);
            }

            control.BringToFront();
        }

        private void SIdeBarTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage selectedTab = SideBarTabControl.SelectedTab;

            if (selectedTab.Name == "TabPageStudentEntry")
            {
                ShowUserControl(new StudentEntryControl(), TabPageStudentEntry);
            } 
            else if (selectedTab.Name == "TabPageSubjectEntry")
            {
                ShowUserControl(new SubjectEntryControl(), TabPageSubjectEntry);
            }
            else if (selectedTab.Name == "TabPageScheduleEntry")
            {
                ShowUserControl(new ScheduleEntryControl(), TabPageScheduleEntry);
            }
            else if (selectedTab.Name == "TabPageEnrollmentEntry")
            {
                ShowUserControl(new EnrollmentEntryControl(), TabPageEnrollmentEntry);
            }
            else if (selectedTab.Name == "TabPageStudentGrade")
            {
                ShowUserControl(new StudentGradeControl(), TabPageStudentGrade);
            }
        }
    }
}
