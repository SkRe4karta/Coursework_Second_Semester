using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public class EditDatabaseForm : Form
    {
        private DataGridView dgv;
        private Button btnSave, btnClose, btnDeleteRow;
        private string dbPath;
        private SQLiteDataAdapter dataAdapter;
        private DataTable dataTable;

        public EditDatabaseForm(string databasePath)
        {
            dbPath = databasePath;

            Text = "Редактирование базы данных";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 500,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            btnSave = new Button
            {
                Text = "Сохранить",
                Left = 600,
                Top = 510,
                Width = 90
            };
            btnSave.Click += BtnSave_Click;

            btnDeleteRow = new Button
            {
                Text = "Удалить строку",
                Left = 500,
                Top = 510,
                Width = 90
            };
            btnDeleteRow.Click += BtnDeleteRow_Click;

            btnClose = new Button
            {
                Text = "Закрыть",
                Left = 700,
                Top = 510,
                Width = 90
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(dgv);
            Controls.Add(btnSave);
            Controls.Add(btnDeleteRow);
            Controls.Add(btnClose);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                conn.Open();

                dataAdapter = new SQLiteDataAdapter("SELECT * FROM LessonsPlan", conn);
                var commandBuilder = new SQLiteCommandBuilder(dataAdapter);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                dgv.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
                Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                dgv.EndEdit();
                dataAdapter.Update(dataTable);
                MessageBox.Show("Изменения сохранены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения данных: " + ex.Message);
            }
        }

        private void BtnDeleteRow_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Выберите строку для удаления.");
                return;
            }

            var result = MessageBox.Show("Удалить выбранную строку?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    dgv.Rows.RemoveAt(dgv.CurrentRow.Index);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении строки: " + ex.Message);
                }
            }
        }
    }
}
