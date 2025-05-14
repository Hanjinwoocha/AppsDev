using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnrollmentSystem.Repo;

namespace EnrollmentSystem.Controller
{
    public partial class StudentGradeControl : UserControl
    {
        private StudentGradeData repo;
        public StudentGradeControl()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
            repo = new StudentGradeData(connectionString);
        }

        private void IDNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers and control keys
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // On Enter, fetch and display data
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (int.TryParse(IDNumberTextBox.Text.Trim(), out int studentId))
                {
                    // Check if student exists
                    var studentEntryRepo = new EnrollmentSystem.Repo.StudentEntryData(System.Configuration.ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString);
                    if (!studentEntryRepo.IsStudentIdExists(studentId))
                    {
                        MessageBox.Show("Student does not exist.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        StudentGradeDataGridView.Rows.Clear();
                        return;
                    }

                    var rows = repo.GetEnrollmentDetails(studentId);
                    // Check if student has any enrolled subjects
                    if (rows.Count == 0)
                    {
                        MessageBox.Show("This student is not enrolled in any subjects yet.", "No Enrollments", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        StudentGradeDataGridView.Rows.Clear();
                        return;
                    }

                    StudentGradeDataGridView.Rows.Clear();
                    foreach (var row in rows)
                    {
                        StudentGradeDataGridView.Rows.Add(row.StudentID, row.SubjectCode, row.EDPCode, row.Grade.ToString("0.0"), row.Remarks);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric Student ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void StudentGradeDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Only handle GradeColumn
            if (e.RowIndex >= 0 && StudentGradeDataGridView.Columns[e.ColumnIndex].Name == "GradeColumn")
            {
                var cell = StudentGradeDataGridView.Rows[e.RowIndex].Cells["GradeColumn"];
                var remarksCell = StudentGradeDataGridView.Rows[e.RowIndex].Cells["RemarksColumn"];
                string gradeStr = cell.Value?.ToString() ?? "";
                if (double.TryParse(gradeStr, out double grade))
                {
                    if (grade >= 1.0 && grade <= 3.0)
                        remarksCell.Value = "Passed";
                    else if (grade > 3.0 && grade <= 5.0)
                        remarksCell.Value = "Failed";
                    else
                        remarksCell.Value = "Not Graded";
                }
                else
                {
                    remarksCell.Value = "Not Graded";
                }
            }
        }

        private bool ValidateGrade(string gradeStr, out double grade)
        {
            grade = 0.0;
            if (!double.TryParse(gradeStr, out grade))
                return false;
            if (grade < 1.0 || grade > 5.0)
                return false;
            if (gradeStr.Contains("."))
            {
                var decimals = gradeStr.Split('.')[1];
                if (decimals.Length > 2)
                    return false;
            }
            return true;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var grades = new List<StudentGradeRow>();
            foreach (DataGridViewRow row in StudentGradeDataGridView.Rows)
            {
                if (row.IsNewRow) continue;
                int studentId = 0;
                double grade = 0.0;
                string remarks = row.Cells["RemarksColumn"].Value?.ToString() ?? "Not Graded";
                string gradeStr = row.Cells["GradeColumn"].Value?.ToString() ?? "";
                if (!int.TryParse(row.Cells["StudentIDColumn"].Value?.ToString(), out studentId) ||
                    string.IsNullOrWhiteSpace(row.Cells["SubjectCodeColumn"].Value?.ToString()) ||
                    string.IsNullOrWhiteSpace(row.Cells["EDPCodeColumn"].Value?.ToString()))
                {
                    MessageBox.Show("Missing required data in one or more rows.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Grade validation
                if (!ValidateGrade(gradeStr, out grade))
                {
                    MessageBox.Show("All grades must be numbers between 1.0 and 5.0, with at most 2 decimal places.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                grades.Add(new StudentGradeRow
                {
                    StudentID = studentId,
                    SubjectCode = row.Cells["SubjectCodeColumn"].Value.ToString(),
                    EDPCode = row.Cells["EDPCodeColumn"].Value.ToString(),
                    Grade = grade,
                    Remarks = remarks
                });
            }
            if (grades.Count == 0)
            {
                MessageBox.Show("No grades to save.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                repo.SaveGrades(grades);
                MessageBox.Show("Grades saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StudentGradeDataGridView.Rows.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving grades: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
