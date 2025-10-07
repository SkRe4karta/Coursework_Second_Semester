using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public class CreateDatabaseForm : Form
    {
        private DataGridView dgv;
        private Button btnCreateDb;

        public string CreatedDbPath { get; private set; }

        public CreateDatabaseForm()
        {
            Text = "Создание новой базы данных";
            Width = 700;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;

            dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.Columns.Add("Class", "Класс");
            dgv.Columns.Add("Subject", "Предмет");
            dgv.Columns.Add("Teacher", "Учитель");
            dgv.Columns.Add("Room", "Кабинет");
            dgv.Columns.Add("LessonsCount", "Кол-во уроков");

            btnCreateDb = new Button
            {
                Text = "Создать базу",
                Dock = DockStyle.Bottom,
                Height = 40
            };

            btnCreateDb.Click += BtnCreateDb_Click;

            Controls.Add(dgv);
            Controls.Add(btnCreateDb);
        }

        private void BtnCreateDb_Click(object sender, EventArgs e)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("Введите данные хотя бы для одной записи.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "SQLite Database (*.sqlite)|*.sqlite",
                Title = "Сохранить базу данных",
                FileName = "schedule.sqlite"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string path = sfd.FileName;
                        CreateAndFillDatabase(path);
                        CreatedDbPath = path;
                        MessageBox.Show("База данных создана успешно.");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка создания базы: " + ex.Message);
                    }
                }
            }
        }

        private void CreateAndFillDatabase(string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            SQLiteConnection.CreateFile(path);

            using (var conn = new SQLiteConnection($"Data Source={path};Version=3;"))
            {
                conn.Open();

                string createTable = @"
                CREATE TABLE IF NOT EXISTS LessonsPlan (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Class TEXT NOT NULL,
                    Subject TEXT NOT NULL,
                    Teacher TEXT NOT NULL,
                    Room TEXT NOT NULL,
                    LessonsCount INTEGER NOT NULL
                );";

                using (var cmd = new SQLiteCommand(createTable, conn))
                    cmd.ExecuteNonQuery();

                using (var transaction = conn.BeginTransaction())
                {
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string cls = row.Cells["Class"].Value?.ToString();
                        string subject = row.Cells["Subject"].Value?.ToString();
                        string teacher = row.Cells["Teacher"].Value?.ToString();
                        string room = row.Cells["Room"].Value?.ToString();
                        string lessonsCountStr = row.Cells["LessonsCount"].Value?.ToString();

                        if (string.IsNullOrWhiteSpace(cls) || string.IsNullOrWhiteSpace(subject) ||
                            string.IsNullOrWhiteSpace(teacher) || string.IsNullOrWhiteSpace(room) ||
                            !int.TryParse(lessonsCountStr, out int lessonsCount))
                        {
                            throw new Exception("Некорректные данные в таблице.");
                        }

                        string insert = "INSERT INTO LessonsPlan (Class, Subject, Teacher, Room, LessonsCount) VALUES (@c, @s, @t, @r, @l)";
                        using (var cmd = new SQLiteCommand(insert, conn))
                        {
                            cmd.Parameters.AddWithValue("@c", cls);
                            cmd.Parameters.AddWithValue("@s", subject);
                            cmd.Parameters.AddWithValue("@t", teacher);
                            cmd.Parameters.AddWithValue("@r", room);
                            cmd.Parameters.AddWithValue("@l", lessonsCount);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }
    }
}
