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
            LoadUniversityLogo();
            MissionLabel.Text = "To cultivate excellence in education, innovation, and character development\n" +
                               "by fostering a dynamic learning environment where students are empowered\n" +
                               "to achieve academic mastery, professional success, and personal growth—\n" +
                               "inspired by the spirit of resilience and adaptability, much like the perfect\n" +
                               "tonkatsu: crispy on the outside, tender on the inside, and always satisfying.";
            MissionLabel.TextAlign = ContentAlignment.MiddleCenter;

            VisionLabel.Text = "To be a globally recognized institution where knowledge meets creativity,\n" +
                              "producing graduates who are not only skilled professionals but also\n" +
                              "compassionate leaders—ready to make a positive impact on society.";
            VisionLabel.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void LoadUniversityLogo()
        {
            try
            {
                string imagePath = System.IO.Path.Combine(Application.StartupPath, "Images", "university_logo.png");
                if (System.IO.File.Exists(imagePath))
                {
                    UniversityLogoPictureBox.Image = Image.FromFile(imagePath);
                    UniversityLogoPictureBox.SizeMode = PictureBoxSizeMode.Zoom; // This will maintain aspect ratio
                }
                else
                {
                    MessageBox.Show("University logo image not found.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading university logo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
