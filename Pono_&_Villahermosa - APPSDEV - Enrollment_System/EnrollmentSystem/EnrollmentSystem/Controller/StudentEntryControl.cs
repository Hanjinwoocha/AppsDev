using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnrollmentSystem.Repo;
using System.Configuration;
using System.Text.RegularExpressions;

namespace EnrollmentSystem.Controller
{
    public partial class StudentEntryControl : UserControl
    {
        public StudentEntryControl()
        {
            InitializeComponent();
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StudentIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                CourseComboBox.SelectedItem == null ||
                YearComboBox.SelectedItem == null ||
                RemarksComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Regex nameRegex = new Regex(@"^[A-Za-z\s-]+$");
            if (!nameRegex.IsMatch(FirstNameTextBox.Text.Trim()))
            {
                MessageBox.Show("First name must contain only letters, spaces, or hyphens.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!nameRegex.IsMatch(MiddleNameTextBox.Text.Trim()))
            {
                MessageBox.Show("Middle name must contain only letters, spaces, or hyphens.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!nameRegex.IsMatch(LastNameTextBox.Text.Trim()))
            {
                MessageBox.Show("Last name must contain only letters, spaces, or hyphens.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(StudentIDTextBox.Text, out int studentId))
            {
                MessageBox.Show("Student ID must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(YearComboBox.SelectedItem.ToString(), out int year))
            {
                MessageBox.Show("Year must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
            var data = new StudentEntryData(connectionString);

            if (data.IsStudentIdExists(studentId))
            {
                MessageBox.Show("Student ID already exists. Please enter a different ID.", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                StudentIDTextBox.Focus();
                return;
            }

            string lastName = LastNameTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string middleName = MiddleNameTextBox.Text.Trim();
            string course = CourseComboBox.SelectedItem.ToString();
            string remarks = RemarksComboBox.SelectedItem.ToString();

            bool success = data.SaveStudent(studentId, lastName, firstName, middleName, course, year, remarks);

            if (success)
            {
                MessageBox.Show("Entries Recorded!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Optionally clear fields here
            }
        }
    }
}
