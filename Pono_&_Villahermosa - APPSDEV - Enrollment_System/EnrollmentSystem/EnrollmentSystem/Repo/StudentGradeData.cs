using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace EnrollmentSystem.Repo
{
    internal class StudentGradeData
    {
        private readonly string connectionString;
        public StudentGradeData(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Fetch enrollment details for a student
        public List<StudentGradeRow> GetEnrollmentDetails(int studentId)
        {
            var result = new List<StudentGradeRow>();
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                // Get subject codes for the student
                string query = "SELECT ENRDFSTUDSUBJCODE FROM EnrollmentDetailFile WHERE ENRDFSTUDID = ?";
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ENRDFSTUDID", studentId);
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subjCode = reader["ENRDFSTUDSUBJCODE"].ToString();
                            // Get EDP code for this subject
                            string edpCode = GetEDPCodeForSubject(subjCode, conn);
                            // Fetch grade and remarks from StudentGradeFile
                            double grade = 0.0;
                            string remarks = "Not Graded";
                            string gradeQuery = "SELECT SGFSTUDSUBJGRADE, SGFSTUDREMARKS FROM StudentGradeFile WHERE SGFSTUDID = ? AND SGFSTUDSUBJCODE = ?";
                            using (OleDbCommand gradeCmd = new OleDbCommand(gradeQuery, conn))
                            {
                                gradeCmd.Parameters.AddWithValue("@SGFSTUDID", studentId);
                                gradeCmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", subjCode);
                                using (OleDbDataReader gradeReader = gradeCmd.ExecuteReader())
                                {
                                    if (gradeReader.Read())
                                    {
                                        double.TryParse(gradeReader["SGFSTUDSUBJGRADE"].ToString(), out grade);
                                        remarks = gradeReader["SGFSTUDREMARKS"].ToString();
                                    }
                                }
                            }
                            result.Add(new StudentGradeRow
                            {
                                StudentID = studentId,
                                SubjectCode = subjCode,
                                EDPCode = edpCode,
                                Grade = grade,
                                Remarks = remarks
                            });
                        }
                    }
                }
            }
            return result;
        }

        // Helper: Get EDP code for a subject
        private string GetEDPCodeForSubject(string subjectCode, OleDbConnection conn)
        {
            // Get latest school year and offering for the subject
            string schoolYear = "";
            string offering = "";
            // Get school year from SubjectSchedFile
            string query1 = "SELECT TOP 1 SSFSCHOOLYEAR FROM SubjectSchedFile WHERE SSFSUBJCODE = ? ORDER BY SSFSCHOOLYEAR DESC";
            using (OleDbCommand cmd1 = new OleDbCommand(query1, conn))
            {
                cmd1.Parameters.AddWithValue("@SSFSUBJCODE", subjectCode);
                var result = cmd1.ExecuteScalar();
                if (result != null)
                    schoolYear = result.ToString();
            }
            // Get offering from SubjectFile
            string query2 = "SELECT SFSUBJREGOFRNG FROM SubjectFile WHERE SFSUBJCODE = ?";
            using (OleDbCommand cmd2 = new OleDbCommand(query2, conn))
            {
                cmd2.Parameters.AddWithValue("@SFSUBJCODE", subjectCode);
                var result = cmd2.ExecuteScalar();
                if (result != null)
                    offering = result.ToString();
            }
            if (!string.IsNullOrEmpty(schoolYear) && !string.IsNullOrEmpty(offering) && schoolYear.Length >= 2)
            {
                // Split by dash and take the first year
                string[] years = schoolYear.Split('-');
                string firstYear = years[0].Trim();
                string last2 = firstYear.Length >= 2 ? firstYear.Substring(firstYear.Length - 2) : firstYear;
                return last2 + offering;
            }
            return "";
        }

        // Save grades to StudentGradeFile
        public bool SaveGrades(List<StudentGradeRow> grades)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                using (OleDbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var row in grades)
                        {
                            // Check if grade already exists
                            string checkQuery = "SELECT COUNT(*) FROM StudentGradeFile WHERE SGFSTUDID = ? AND SGFSTUDSUBJCODE = ?";
                            using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@SGFSTUDID", row.StudentID);
                                checkCmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", row.SubjectCode);
                                int count = (int)checkCmd.ExecuteScalar();

                                if (count > 0)
                                {
                                    // Update existing grade
                                    string updateQuery = "UPDATE StudentGradeFile SET SGFSTUDSUBJGRADE = ?, SGFSTUDEDPCODE = ?, SGFSTUDREMARKS = ? WHERE SGFSTUDID = ? AND SGFSTUDSUBJCODE = ?";
                                    using (OleDbCommand updateCmd = new OleDbCommand(updateQuery, conn, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@SGFSTUDSUBJGRADE", row.Grade);
                                        updateCmd.Parameters.AddWithValue("@SGFSTUDEDPCODE", row.EDPCode);
                                        updateCmd.Parameters.AddWithValue("@SGFSTUDREMARKS", row.Remarks);
                                        updateCmd.Parameters.AddWithValue("@SGFSTUDID", row.StudentID);
                                        updateCmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", row.SubjectCode);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // Insert new grade
                                    string insertQuery = "INSERT INTO StudentGradeFile (SGFSTUDID, SGFSTUDSUBJCODE, SGFSTUDSUBJGRADE, SGFSTUDEDPCODE, SGFSTUDREMARKS) VALUES (?, ?, ?, ?, ?)";
                                    using (OleDbCommand insertCmd = new OleDbCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@SGFSTUDID", row.StudentID);
                                        insertCmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", row.SubjectCode);
                                        insertCmd.Parameters.AddWithValue("@SGFSTUDSUBJGRADE", row.Grade);
                                        insertCmd.Parameters.AddWithValue("@SGFSTUDEDPCODE", row.EDPCode);
                                        insertCmd.Parameters.AddWithValue("@SGFSTUDREMARKS", row.Remarks);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    // Helper class for grid rows
    public class StudentGradeRow
    {
        public int StudentID { get; set; }
        public string SubjectCode { get; set; }
        public string EDPCode { get; set; }
        public double Grade { get; set; }
        public string Remarks { get; set; }
    }
}
