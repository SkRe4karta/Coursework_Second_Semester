using System;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public class LoginForm : Form
    {
        private TextBox tbUser, tbClass;
        private Button btnLogin;
        private Label lblUser, lblClass;

        public LoginForm()
        {
            Text = "Вход";
            Width = 300;
            Height = 190;
            StartPosition = FormStartPosition.CenterScreen;

            lblUser = new Label { Text = "Пользователь (admin или user)", Left = 10, Top = 15, Width = 260 };
            tbUser = new TextBox { Left = 10, Top = 35, Width = 260 };

            lblClass = new Label { Text = "Класс (только для user)", Left = 10, Top = 65, Width = 260 };
            tbClass = new TextBox { Left = 10, Top = 85, Width = 260 };

            btnLogin = new Button { Text = "Войти", Left = 10, Top = 120, Width = 260 };
            btnLogin.Click += BtnLogin_Click;

            Controls.AddRange(new Control[] { lblUser, tbUser, lblClass, tbClass, btnLogin });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = tbUser.Text.Trim().ToLower();
            var cls = tbClass.Text.Trim();

            if (user == "admin")
            {
                Hide();
                var adminForm = new AdminForm();
                adminForm.ShowDialog();
                Show();
            }
            else if (user == "user")
            {
                if (string.IsNullOrEmpty(cls))
                {
                    MessageBox.Show("Введите класс для пользователя");
                    return;
                }
                Hide();
                var userForm = new UserForm(cls);
                userForm.ShowDialog();
                Show();
            }
            else
            {
                MessageBox.Show("Неверный пользователь. Введите 'admin' или 'user'");
            }
        }
    }
}
