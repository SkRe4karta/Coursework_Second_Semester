using System;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public class TeacherReplaceForm : Form
    {
        private ComboBox cbTeachers;
        private Button btnOk;
        private Button btnCancel;
        private Label lblPrompt;

        public string SelectedTeacher { get; private set; }

        public TeacherReplaceForm(string[] teachers)
        {
            Text = "Выбор нового учителя";
            Width = 350;
            Height = 150;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            lblPrompt = new Label()
            {
                Text = "Выберите учителя для замены:",
                Left = 10,
                Top = 10,
                Width = 310
            };

            cbTeachers = new ComboBox()
            {
                Left = 10,
                Top = 35,
                Width = 310,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbTeachers.Items.AddRange(teachers);
            if (teachers.Length > 0)
                cbTeachers.SelectedIndex = 0;

            btnOk = new Button()
            {
                Text = "ОК",
                Left = 160,
                Top = 70,
                Width = 75,
                DialogResult = DialogResult.OK
            };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button()
            {
                Text = "Отмена",
                Left = 245,
                Top = 70,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(lblPrompt);
            Controls.Add(cbTeachers);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (cbTeachers.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите учителя.");
                this.DialogResult = DialogResult.None;
                return;
            }

            SelectedTeacher = cbTeachers.SelectedItem.ToString();
        }
    }
}
