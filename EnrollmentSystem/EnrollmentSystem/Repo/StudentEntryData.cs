using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace EnrollmentSystem.Repo
{
    internal class StudentEntryData
    {
        private readonly string connectionString;
        public StudentEntryData(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool IsStudentIdExists(int studentId)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "SELECT COUNT(*) FROM StudentFile WHERE STFSTUDID = ?";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@STFSTUDID", studentId);
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking student ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool SaveStudent(int studentId, string lastName, string firstName, string middleName, string course, int year, string remarks)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "INSERT INTO StudentFile (STFSTUDID, STFSTUDLNAME, STFSTUDFNAME, STFSTUDMNAME, STFSTUDCOURSE, STFSTUDYEAR, STFSTUDREMARKS, STFSTUDSTATUS) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@STFSTUDID", studentId);
                        cmd.Parameters.AddWithValue("@STFSTUDLNAME", lastName);
                        cmd.Parameters.AddWithValue("@STFSTUDFNAME", firstName);
                        cmd.Parameters.AddWithValue("@STFSTUDMNAME", middleName);
                        cmd.Parameters.AddWithValue("@STFSTUDCOURSE", course);
                        cmd.Parameters.AddWithValue("@STFSTUDYEAR", year);
                        cmd.Parameters.AddWithValue("@STFSTUDREMARKS", remarks);
                        cmd.Parameters.AddWithValue("@STFSTUDSTATUS", "AC");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving student: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
