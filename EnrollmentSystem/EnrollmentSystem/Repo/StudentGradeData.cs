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
                string last2 = schoolYear.Substring(schoolYear.Length - 2);
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
                foreach (var row in grades)
                {
                    string query = "INSERT INTO StudentGradeFile (SGFSTUDID, SGFSTUDSUBJCODE, SGFSTUDSUBJGRADE, SGFSTUDEDPCODE, SGFSTUDREMARKS) VALUES (?, ?, ?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SGFSTUDID", row.StudentID);
                        cmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", row.SubjectCode);
                        cmd.Parameters.AddWithValue("@SGFSTUDSUBJGRADE", row.Grade);
                        cmd.Parameters.AddWithValue("@SGFSTUDEDPICODE", row.EDPCode);
                        cmd.Parameters.AddWithValue("@SGFSTUDREMARKS", row.Remarks);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return true;
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
