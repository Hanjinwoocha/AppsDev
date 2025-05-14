using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrollmentSystem.Repo
{
    internal class SubjectEntryData
    {
        private readonly string connectionString;
        public SubjectEntryData(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool AddSubject(string code, string desc, double units, int offering, string category, string status, string courseCode, string currCode)
        {
            try
            {
                if (IsSubjectCodeExists(code))
                {
                    System.Windows.Forms.MessageBox.Show($"Subject code '{code}' already exists. Please use a unique code.", "Duplicate Subject Code", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return false;
                }

                if (!IsCourseCodeExists(courseCode))
                {
                    System.Windows.Forms.MessageBox.Show($"Course code '{courseCode}' does not exist.", "Invalid Course Code", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return false;
                }

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "INSERT INTO SubjectFile (SFSUBJCODE, SFSUBJDESC, SFSUBJUNITS, SFSUBJREGOFRNG, SFSUBJCATEGORY, SFSUBJSTATUS, SFSUBJCOURSECODE, SFSUBJCURRCODE) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SFSUBJCODE", code);
                        cmd.Parameters.AddWithValue("@SFSUBJDESC", desc);
                        cmd.Parameters.AddWithValue("@SFSUBJUNITS", units);
                        cmd.Parameters.AddWithValue("@SFSUBJREGOFRNG", offering);
                        cmd.Parameters.AddWithValue("@SFSUBJCATEGORY", category);
                        cmd.Parameters.AddWithValue("@SFSUBJSTATUS", status);
                        cmd.Parameters.AddWithValue("@SFSUBJCOURSECODE", courseCode);
                        cmd.Parameters.AddWithValue("@SFSUBJCURRCODE", currCode);
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error adding subject: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        public DataRow FindSubjectByCode(string code)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "SELECT SFSUBJCODE, SFSUBJDESC, SFSUBJUNITS FROM SubjectFile WHERE SFSUBJCODE = ?";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SFSUBJCODE", code);
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt.Rows.Count > 0)
                                return dt.Rows[0];
                            else
                                return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error finding subject: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
        }

        public bool AddRequisite(string subjCode, string preqCode, string category)
        {
            try
            {
                if (IsRequisiteExists(subjCode, preqCode, category))
                {
                    System.Windows.Forms.MessageBox.Show($"This prerequisite relationship already exists.", "Duplicate Prerequisite", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return false;
                }

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "INSERT INTO SubjectPreqFile (SUBJCODE, SUBJPRECODE, SUBJCATEGORY) VALUES (?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SUBJCODE", subjCode);
                        cmd.Parameters.AddWithValue("@SUBJPRECODE", preqCode);
                        cmd.Parameters.AddWithValue("@SUBJCATEGORY", category);
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error adding requisite: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        public bool IsSubjectCodeExists(string code)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "SELECT COUNT(*) FROM SubjectFile WHERE SFSUBJCODE = ?";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SFSUBJCODE", code);
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error checking subject code: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        private bool IsCourseCodeExists(string courseCode)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "SELECT COUNT(*) FROM SubjectFile WHERE SFSUBJCOURSECODE = ?";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SFSUBJCOURSECODE", courseCode);
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error checking course code: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        private bool IsRequisiteExists(string subjCode, string preqCode, string category)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string sql = "SELECT COUNT(*) FROM SubjectPreqFile WHERE SUBJCODE = ? AND SUBJPRECODE = ? AND SUBJCATEGORY = ?";
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SUBJCODE", subjCode);
                        cmd.Parameters.AddWithValue("@SUBJPRECODE", preqCode);
                        cmd.Parameters.AddWithValue("@SUBJCATEGORY", category);
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error checking prerequisite: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
