using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }

    // ==================== РАБОТА С БАЗОЙ ДАННЫХ ====================
    public static class DB
    {
        // ИЗМЕНИТЕ ПОД ВАШ СЕРВЕР: localhost, localhost\SQLEXPRESS, (localdb)\MSSQLLocalDB
        private static string cs = @"Server=KALKRAIT-PC\SQLEXPRESS;Database=Cinema;Trusted_Connection=True;TrustServerCertificate=True;";

        public static DataTable Query(string sql, SqlParameter[] p = null)
        {
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (p != null) cmd.Parameters.AddRange(p);
                var dt = new DataTable();
                conn.Open();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static int Exec(string sql, SqlParameter[] p = null)
        {
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (p != null) cmd.Parameters.AddRange(p);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, SqlParameter[] p = null)
        {
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (p != null) cmd.Parameters.AddRange(p);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }

    // ==================== ГЛАВНАЯ ФОРМА ====================
    public class FormMain : Form
    {
        private DataGridView dgvMovies;
        private DataGridView dgvSessions;
        private DataGridView dgvBookings;
        private Button btnAddMovie;
        private Button btnAddSession;
        private Button btnBuyTicket;
        private Button btnCancelBooking;
        private Button btnRefresh;

        public FormMain()
        {
            Text = "Кинотеатр - Система управления";
            Size = new Size(1000, 550);
            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            // Таблицы
            dgvMovies = new DataGridView();
            dgvMovies.Location = new Point(12, 40);
            dgvMovies.Size = new Size(400, 200);

            dgvSessions = new DataGridView();
            dgvSessions.Location = new Point(430, 40);
            dgvSessions.Size = new Size(540, 200);

            dgvBookings = new DataGridView();
            dgvBookings.Location = new Point(12, 290);
            dgvBookings.Size = new Size(958, 200);

            // Кнопки
            btnAddMovie = new Button();
            btnAddMovie.Text = "Добавить фильм";
            btnAddMovie.Location = new Point(12, 250);
            btnAddMovie.Size = new Size(100, 30);
            btnAddMovie.Click += BtnAddMovie_Click;

            btnAddSession = new Button();
            btnAddSession.Text = "Добавить сеанс";
            btnAddSession.Location = new Point(118, 250);
            btnAddSession.Size = new Size(100, 30);
            btnAddSession.Click += BtnAddSession_Click;

            btnBuyTicket = new Button();
            btnBuyTicket.Text = "Купить билет";
            btnBuyTicket.Location = new Point(430, 250);
            btnBuyTicket.Size = new Size(100, 30);
            btnBuyTicket.Click += BtnBuyTicket_Click;

            btnCancelBooking = new Button();
            btnCancelBooking.Text = "Отменить бронь";
            btnCancelBooking.Location = new Point(536, 250);
            btnCancelBooking.Size = new Size(100, 30);
            btnCancelBooking.Click += BtnCancelBooking_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new Point(870, 250);
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Click += BtnRefresh_Click;

            // Метки
            Label lbl1 = new Label();
            lbl1.Text = "Фильмы:";
            lbl1.Font = new Font("Arial", 10, FontStyle.Bold);
            lbl1.Location = new Point(12, 20);
            lbl1.AutoSize = true;

            Label lbl2 = new Label();
            lbl2.Text = "Сеансы:";
            lbl2.Font = new Font("Arial", 10, FontStyle.Bold);
            lbl2.Location = new Point(430, 20);
            lbl2.AutoSize = true;

            Label lbl3 = new Label();
            lbl3.Text = "Брони:";
            lbl3.Font = new Font("Arial", 10, FontStyle.Bold);
            lbl3.Location = new Point(12, 270);
            lbl3.AutoSize = true;

            Controls.Add(dgvMovies);
            Controls.Add(dgvSessions);
            Controls.Add(dgvBookings);
            Controls.Add(btnAddMovie);
            Controls.Add(btnAddSession);
            Controls.Add(btnBuyTicket);
            Controls.Add(btnCancelBooking);
            Controls.Add(btnRefresh);
            Controls.Add(lbl1);
            Controls.Add(lbl2);
            Controls.Add(lbl3);
        }

        private void BtnAddMovie_Click(object sender, EventArgs e)
        {
            FormMovie form = new FormMovie();
            if (form.ShowDialog() == DialogResult.OK)
                LoadMovies();
        }

        private void BtnAddSession_Click(object sender, EventArgs e)
        {
            try
            {
                FormSession form = new FormSession();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadSessions();
                    MessageBox.Show("Сеанс добавлен! Список обновлен.", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы добавления сеанса: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBuyTicket_Click(object sender, EventArgs e)
        {
            if (dgvSessions.CurrentRow == null)
            {
                MessageBox.Show("Выберите сеанс для покупки билета", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.CurrentRow.Cells["session_id"].Value);
            FormBuyTicket form = new FormBuyTicket(sessionId);
            if (form.ShowDialog() == DialogResult.OK)
                LoadBookings();
        }

        private void BtnCancelBooking_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow == null)
            {
                MessageBox.Show("Выберите бронь для отмены", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int bookingId = Convert.ToInt32(dgvBookings.CurrentRow.Cells["booking_id"].Value);
            string status = dgvBookings.CurrentRow.Cells["status"].Value.ToString();

            if (status == "cancelled")
            {
                MessageBox.Show("Эта бронь уже отменена", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Вы уверены, что хотите отменить бронь?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SqlParameter[] param = new SqlParameter[] { new SqlParameter("@id", bookingId) };
                DB.Exec("UPDATE bookings SET status = 'cancelled' WHERE booking_id = @id", param);
                LoadBookings();
                MessageBox.Show("Бронь успешно отменена", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            LoadMovies();
            LoadSessions();
            LoadBookings();
        }

        private void LoadMovies()
        {
            DataTable dt = DB.Query("SELECT movie_id, title, duration_min, genre, age_rating FROM movies ORDER BY title");
            dgvMovies.DataSource = dt;
            dgvMovies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadSessions()
        {
            string sql = @"SELECT s.session_id, m.title AS movie, h.name AS hall, 
                s.date, s.start_time, s.price_standard, s.price_vip 
                FROM sessions s
                JOIN movies m ON s.movie_id = m.movie_id 
                JOIN halls h ON s.hall_id = h.hall_id
                WHERE s.date >= GETDATE() 
                ORDER BY s.date, s.start_time";

            DataTable dt = DB.Query(sql);
            dgvSessions.DataSource = dt;
            dgvSessions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadBookings()
        {
            // ВАЖНО: [users] и [user] экранированы квадратными скобками
            string sql = @"SELECT b.booking_id, u.full_name AS [user], m.title AS movie, 
                s.date, s.start_time, b.total_amount, b.status, b.qr_code 
                FROM bookings b
                JOIN [users] u ON b.user_id = u.user_id 
                JOIN sessions s ON b.session_id = s.session_id
                JOIN movies m ON s.movie_id = m.movie_id 
                ORDER BY b.booking_date DESC";

            DataTable dt = DB.Query(sql);
            dgvBookings.DataSource = dt;
            dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }

    // ==================== ДОБАВЛЕНИЕ ФИЛЬМА ====================
    public class FormMovie : Form
    {
        private TextBox txtTitle;
        private TextBox txtDuration;
        private TextBox txtGenre;
        private TextBox txtDesc;
        private ComboBox cmbAge;
        private DateTimePicker dtpDate;

        public FormMovie()
        {
            Text = "Добавить фильм";
            Size = new Size(400, 380);
            StartPosition = FormStartPosition.CenterParent;

            int y = 20;

            // Название
            Label lblTitle = new Label();
            lblTitle.Text = "Название:";
            lblTitle.Location = new Point(20, y + 3);
            lblTitle.AutoSize = true;

            txtTitle = new TextBox();
            txtTitle.Location = new Point(120, y);
            txtTitle.Size = new Size(250, 20);
            Controls.Add(lblTitle);
            Controls.Add(txtTitle);

            // Длительность
            y += 35;
            Label lblDuration = new Label();
            lblDuration.Text = "Длительность (мин):";
            lblDuration.Location = new Point(20, y + 3);
            lblDuration.AutoSize = true;

            txtDuration = new TextBox();
            txtDuration.Location = new Point(120, y);
            txtDuration.Size = new Size(100, 20);
            Controls.Add(lblDuration);
            Controls.Add(txtDuration);

            // Жанр
            y += 35;
            Label lblGenre = new Label();
            lblGenre.Text = "Жанр:";
            lblGenre.Location = new Point(20, y + 3);
            lblGenre.AutoSize = true;

            txtGenre = new TextBox();
            txtGenre.Location = new Point(120, y);
            txtGenre.Size = new Size(150, 20);
            Controls.Add(lblGenre);
            Controls.Add(txtGenre);

            // Дата выхода
            y += 35;
            Label lblDate = new Label();
            lblDate.Text = "Дата выхода:";
            lblDate.Location = new Point(20, y + 3);
            lblDate.AutoSize = true;

            dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(120, y);
            dtpDate.Size = new Size(150, 20);
            Controls.Add(lblDate);
            Controls.Add(dtpDate);

            // Возрастной рейтинг
            y += 35;
            Label lblAge = new Label();
            lblAge.Text = "Возраст:";
            lblAge.Location = new Point(20, y + 3);
            lblAge.AutoSize = true;

            cmbAge = new ComboBox();
            cmbAge.Location = new Point(120, y);
            cmbAge.Size = new Size(80, 21);
            cmbAge.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAge.Items.AddRange(new object[] { "0+", "6+", "12+", "16+", "18+" });
            cmbAge.SelectedIndex = 0;
            Controls.Add(lblAge);
            Controls.Add(cmbAge);

            // Описание
            y += 35;
            Label lblDesc = new Label();
            lblDesc.Text = "Описание:";
            lblDesc.Location = new Point(20, y);
            lblDesc.AutoSize = true;

            y += 25;
            txtDesc = new TextBox();
            txtDesc.Location = new Point(20, y);
            txtDesc.Size = new Size(350, 80);
            txtDesc.Multiline = true;
            Controls.Add(lblDesc);
            Controls.Add(txtDesc);

            // Кнопки
            y += 90;
            Button btnSave = new Button();
            btnSave.Text = "Сохранить";
            btnSave.Location = new Point(100, y);
            btnSave.Size = new Size(100, 30);
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(220, y);
            btnCancel.Size = new Size(100, 30);
            btnCancel.Click += (s, e) => Close();

            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                MessageBox.Show("Введите название фильма", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtDuration.Text))
            {
                MessageBox.Show("Введите длительность фильма", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@t", txtTitle.Text),
                new SqlParameter("@d", int.Parse(txtDuration.Text)),
                new SqlParameter("@g", txtGenre.Text),
                new SqlParameter("@rd", dtpDate.Value),
                new SqlParameter("@a", cmbAge.Text),
                new SqlParameter("@desc", txtDesc.Text)
            };

            DB.Exec(@"INSERT INTO movies (title, duration_min, genre, release_date, age_rating, description) 
                VALUES (@t, @d, @g, @rd, @a, @desc)", param);

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // ==================== ДОБАВЛЕНИЕ СЕАНСА ====================
    // ==================== ДОБАВЛЕНИЕ СЕАНСА (ИСПРАВЛЕННЫЙ) ====================
    public class FormSession : Form
    {
        private ComboBox cmbMovie;
        private ComboBox cmbHall;
        private DateTimePicker dtpDate;
        private DateTimePicker dtpTime;
        private NumericUpDown numStd;
        private NumericUpDown numVip;
        private CheckBox chk3d;

        public FormSession()
        {
            Text = "Добавить сеанс";
            Size = new Size(400, 400);
            StartPosition = FormStartPosition.CenterParent;

            int y = 20;

            // Фильм
            Label lblMovie = new Label();
            lblMovie.Text = "Фильм:";
            lblMovie.Location = new Point(20, y + 3);
            lblMovie.AutoSize = true;

            cmbMovie = new ComboBox();
            cmbMovie.Location = new Point(120, y);
            cmbMovie.Size = new Size(250, 21);
            cmbMovie.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(lblMovie);
            Controls.Add(cmbMovie);

            // Зал
            y += 35;
            Label lblHall = new Label();
            lblHall.Text = "Зал:";
            lblHall.Location = new Point(20, y + 3);
            lblHall.AutoSize = true;

            cmbHall = new ComboBox();
            cmbHall.Location = new Point(120, y);
            cmbHall.Size = new Size(150, 21);
            cmbHall.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(lblHall);
            Controls.Add(cmbHall);

            // Дата
            y += 35;
            Label lblDate = new Label();
            lblDate.Text = "Дата:";
            lblDate.Location = new Point(20, y + 3);
            lblDate.AutoSize = true;

            dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(120, y);
            dtpDate.Size = new Size(150, 20);
            dtpDate.Value = DateTime.Today.AddDays(1);
            Controls.Add(lblDate);
            Controls.Add(dtpDate);

            // Время
            y += 35;
            Label lblTime = new Label();
            lblTime.Text = "Время:";
            lblTime.Location = new Point(20, y + 3);
            lblTime.AutoSize = true;

            dtpTime = new DateTimePicker();
            dtpTime.Location = new Point(120, y);
            dtpTime.Size = new Size(100, 20);
            dtpTime.Format = DateTimePickerFormat.Time;
            dtpTime.ShowUpDown = true;
            dtpTime.Value = DateTime.Today.AddHours(10);
            Controls.Add(lblTime);
            Controls.Add(dtpTime);

            // Цена обычная
            y += 35;
            Label lblStd = new Label();
            lblStd.Text = "Цена обыч. (руб):";
            lblStd.Location = new Point(20, y + 3);
            lblStd.AutoSize = true;

            numStd = new NumericUpDown();
            numStd.Location = new Point(120, y);
            numStd.Minimum = 100;
            numStd.Maximum = 2000;
            numStd.Value = 350;
            numStd.DecimalPlaces = 2;
            Controls.Add(lblStd);
            Controls.Add(numStd);

            // Цена VIP
            y += 35;
            Label lblVip = new Label();
            lblVip.Text = "Цена VIP (руб):";
            lblVip.Location = new Point(20, y + 3);
            lblVip.AutoSize = true;

            numVip = new NumericUpDown();
            numVip.Location = new Point(120, y);
            numVip.Minimum = 100;
            numVip.Maximum = 3000;
            numVip.Value = 500;
            numVip.DecimalPlaces = 2;
            Controls.Add(lblVip);
            Controls.Add(numVip);

            // 3D
            y += 35;
            chk3d = new CheckBox();
            chk3d.Text = "3D сеанс";
            chk3d.Location = new Point(120, y);
            Controls.Add(chk3d);

            // Кнопки
            y += 45;
            Button btnSave = new Button();
            btnSave.Text = "Сохранить";
            btnSave.Location = new Point(100, y);
            btnSave.Size = new Size(100, 30);
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(220, y);
            btnCancel.Size = new Size(100, 30);
            btnCancel.Click += (s, e) => Close();

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            // Загрузка данных
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Загрузка фильмов
                DataTable dtMovies = DB.Query("SELECT movie_id, title FROM movies ORDER BY title");
                if (dtMovies.Rows.Count == 0)
                {
                    MessageBox.Show("Нет фильмов в базе данных. Сначала добавьте фильмы.",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                cmbMovie.DataSource = dtMovies;
                cmbMovie.DisplayMember = "title";
                cmbMovie.ValueMember = "movie_id";

                // Загрузка залов
                DataTable dtHalls = DB.Query("SELECT hall_id, name FROM halls ORDER BY name");
                if (dtHalls.Rows.Count == 0)
                {
                    MessageBox.Show("Нет залов в базе данных. Сначала добавьте залы.",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                cmbHall.DataSource = dtHalls;
                cmbHall.DisplayMember = "name";
                cmbHall.ValueMember = "hall_id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка выбора
                if (cmbMovie.SelectedValue == null)
                {
                    MessageBox.Show("Выберите фильм", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbHall.SelectedValue == null)
                {
                    MessageBox.Show("Выберите зал", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Получаем ID выбранных значений
                int movieId = Convert.ToInt32(cmbMovie.SelectedValue);
                int hallId = Convert.ToInt32(cmbHall.SelectedValue);

                // Получаем длительность фильма
                string durationQuery = "SELECT duration_min FROM movies WHERE movie_id = @id";
                SqlParameter[] durationParam = new SqlParameter[] { new SqlParameter("@id", movieId) };
                object durationResult = DB.Scalar(durationQuery, durationParam);

                if (durationResult == null)
                {
                    MessageBox.Show("Не удалось получить длительность фильма", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int duration = Convert.ToInt32(durationResult);

                // Рассчитываем время окончания
                DateTime startDateTime = dtpDate.Value.Date + dtpTime.Value.TimeOfDay;
                DateTime endDateTime = startDateTime.AddMinutes(duration);
                TimeSpan startTime = dtpTime.Value.TimeOfDay;
                TimeSpan endTime = endDateTime.TimeOfDay;

                // SQL запрос на вставку
                string insertSql = @"
                    INSERT INTO sessions (hall_id, movie_id, start_time, end_time, price_standard, price_vip, is_3d, date)
                    VALUES (@hall_id, @movie_id, @start_time, @end_time, @price_standard, @price_vip, @is_3d, @date)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@hall_id", hallId),
                    new SqlParameter("@movie_id", movieId),
                    new SqlParameter("@start_time", startTime),
                    new SqlParameter("@end_time", endTime),
                    new SqlParameter("@price_standard", numStd.Value),
                    new SqlParameter("@price_vip", numVip.Value),
                    new SqlParameter("@is_3d", chk3d.Checked ? 1 : 0),
                    new SqlParameter("@date", dtpDate.Value.Date)
                };

                // Выполняем вставку
                int rowsAffected = DB.Exec(insertSql, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"Сеанс успешно добавлен!\n\n" +
                        $"Фильм: {cmbMovie.Text}\n" +
                        $"Зал: {cmbHall.Text}\n" +
                        $"Дата: {dtpDate.Value.ToShortDateString()}\n" +
                        $"Время: {startTime:hh\\:mm}\n" +
                        $"Длительность: {duration} мин.\n" +
                        $"Цена: {numStd.Value} руб. (обыч.) / {numVip.Value} руб. (VIP)",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить сеанс. Ни одна запись не была добавлена.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Ошибка базы данных: {sqlEx.Message}\n\n" +
                    $"Код ошибки: {sqlEx.Number}\n" +
                    $"Проверьте:\n" +
                    "- Существует ли зал с указанным ID\n" +
                    "- Существует ли фильм с указанным ID\n" +
                    "- Нет ли конфликта времени с другим сеансом в этом зале",
                    "SQL Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сеанса: {ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ==================== ПОКУПКА БИЛЕТА ====================
    public class FormBuyTicket : Form
    {
        private int sessionId;
        private int selectedSeat = -1;
        private Label lblInfo;
        private Label lblSeat;
        private Panel pnlSeats;
        private RadioButton rbStd;
        private RadioButton rbChild;
        private RadioButton rbSenior;
        private RadioButton rbVip;

        public FormBuyTicket(int id)
        {
            sessionId = id;
            Text = "Выбор места";
            Size = new Size(650, 600);
            StartPosition = FormStartPosition.CenterParent;

            // Информация
            lblInfo = new Label();
            lblInfo.Location = new Point(12, 12);
            lblInfo.AutoSize = true;

            // Панель для мест
            pnlSeats = new Panel();
            pnlSeats.Location = new Point(12, 150);
            pnlSeats.Size = new Size(600, 300);
            pnlSeats.BorderStyle = BorderStyle.FixedSingle;
            pnlSeats.AutoScroll = true;

            // Выбранное место
            lblSeat = new Label();
            lblSeat.Location = new Point(12, 470);
            lblSeat.Font = new Font("Arial", 9, FontStyle.Bold);
            lblSeat.AutoSize = true;
            lblSeat.Text = "Место не выбрано";

            // Тип билета
            GroupBox group = new GroupBox();
            group.Text = "Тип билета";
            group.Location = new Point(450, 12);
            group.Size = new Size(150, 120);

            rbStd = new RadioButton();
            rbStd.Text = "Стандартный";
            rbStd.Location = new Point(10, 22);
            rbStd.Checked = true;

            rbChild = new RadioButton();
            rbChild.Text = "Детский";
            rbChild.Location = new Point(10, 45);

            rbSenior = new RadioButton();
            rbSenior.Text = "Пенсионный";
            rbSenior.Location = new Point(10, 68);

            rbVip = new RadioButton();
            rbVip.Text = "VIP";
            rbVip.Location = new Point(10, 91);

            group.Controls.Add(rbStd);
            group.Controls.Add(rbChild);
            group.Controls.Add(rbSenior);
            group.Controls.Add(rbVip);

            // Кнопки
            Button btnBuy = new Button();
            btnBuy.Text = "Купить";
            btnBuy.Location = new Point(450, 520);
            btnBuy.Size = new Size(150, 35);
            btnBuy.Click += BtnBuy_Click;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(280, 520);
            btnCancel.Size = new Size(150, 35);
            btnCancel.Click += (s, e) => Close();

            Controls.Add(lblInfo);
            Controls.Add(pnlSeats);
            Controls.Add(lblSeat);
            Controls.Add(group);
            Controls.Add(btnBuy);
            Controls.Add(btnCancel);

            LoadInfo();
            LoadSeats();
        }

        private void LoadInfo()
        {
            string sql = @"SELECT m.title, h.name AS hall, s.date, s.start_time, s.price_standard, s.price_vip 
                FROM sessions s 
                JOIN movies m ON s.movie_id = m.movie_id 
                JOIN halls h ON s.hall_id = h.hall_id 
                WHERE s.session_id = @id";

            SqlParameter[] param = new SqlParameter[] { new SqlParameter("@id", sessionId) };
            DataTable dt = DB.Query(sql, param);

            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                lblInfo.Text = $"Фильм: {r["title"]}  |  Зал: {r["hall"]}  |  {((DateTime)r["date"]).ToShortDateString()} {r["start_time"]}\n" +
                              $"Обычный: {r["price_standard"]} руб.  |  VIP: {r["price_vip"]} руб.";
            }
        }

        private void LoadSeats()
        {
            string sql = @"SELECT s.seat_id, s.row_num, s.seat_num, s.is_vip, s.is_disabled,
                CASE WHEN b.status IN ('pending','paid') THEN 1 ELSE 0 END AS taken
                FROM seats s 
                JOIN sessions ses ON ses.hall_id = s.hall_id
                LEFT JOIN tickets t ON s.seat_id = t.seat_id AND t.is_returned = 0
                LEFT JOIN booking_tickets bt ON t.ticket_id = bt.ticket_id
                LEFT JOIN bookings b ON bt.booking_id = b.booking_id AND b.session_id = @sid
                WHERE ses.session_id = @sid 
                ORDER BY s.row_num, s.seat_num";

            SqlParameter[] param = new SqlParameter[] { new SqlParameter("@sid", sessionId) };
            DataTable dt = DB.Query(sql, param);

            int currentRow = -1;
            int x = 70;
            int y = 10;

            foreach (DataRow r in dt.Rows)
            {
                int rowNum = Convert.ToInt32(r["row_num"]);

                if (rowNum != currentRow)
                {
                    Label lblRow = new Label();
                    lblRow.Text = $"Ряд {rowNum}";
                    lblRow.Location = new Point(10, y + 5);
                    lblRow.Size = new Size(50, 25);
                    pnlSeats.Controls.Add(lblRow);

                    currentRow = rowNum;
                    x = 70;
                    y += 35;
                }

                int seatId = Convert.ToInt32(r["seat_id"]);
                int seatNum = Convert.ToInt32(r["seat_num"]);
                bool isVip = Convert.ToBoolean(r["is_vip"]);
                bool isTaken = Convert.ToBoolean(r["taken"]);
                bool isDisabled = Convert.ToBoolean(r["is_disabled"]);

                Button btnSeat = new Button();
                btnSeat.Text = seatNum.ToString();
                btnSeat.Size = new Size(40, 35);
                btnSeat.Location = new Point(x, y);
                btnSeat.Tag = seatId;

                if (isDisabled)
                {
                    btnSeat.BackColor = Color.Gray;
                    btnSeat.Enabled = false;
                    btnSeat.Text = "✖";
                }
                else if (isTaken)
                {
                    btnSeat.BackColor = Color.Red;
                    btnSeat.Enabled = false;
                    btnSeat.Text = "Занято";
                    btnSeat.Font = new Font(btnSeat.Font, FontStyle.Bold);
                }
                else if (isVip)
                {
                    btnSeat.BackColor = Color.Gold;
                    btnSeat.ForeColor = Color.Black;
                }
                else
                {
                    btnSeat.BackColor = Color.LightGreen;
                }

                int seatIdCapture = seatId;
                int seatNumCapture = seatNum;
                int rowNumCapture = rowNum;
                bool isVipCapture = isVip;

                btnSeat.Click += (s, e) =>
                {
                    selectedSeat = seatIdCapture;
                    lblSeat.Text = $"Выбрано место: ряд {rowNumCapture}, место {seatNumCapture}{(isVipCapture ? " (VIP)" : "")}";
                };

                pnlSeats.Controls.Add(btnSeat);
                x += 45;
            }
        }

        private void BtnBuy_Click(object sender, EventArgs e)
        {
            if (selectedSeat == -1)
            {
                MessageBox.Show("Выберите место", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ticketType = "standard";
            if (rbChild.Checked) ticketType = "child";
            else if (rbSenior.Checked) ticketType = "senior";
            else if (rbVip.Checked) ticketType = "vip";

            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@uid", 1), // ID пользователя (для демо)
                new SqlParameter("@sid", sessionId),
                new SqlParameter("@seat", selectedSeat),
                new SqlParameter("@type", ticketType)
            };

            try
            {
                DB.Exec("EXEC sp_add_booking @uid, @sid, @seat, @type", param);
                MessageBox.Show("Билет успешно приобретен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при покупке: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}