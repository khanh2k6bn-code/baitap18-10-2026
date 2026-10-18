using System;
using System.Drawing;
using System.Windows.Forms;

namespace HospitalManagerApp
{
    public partial class frmDoctorSchedule : Form
    {
        private DataGridView dgvDoctors;
        private Label lblTitle;

        public frmDoctorSchedule()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Danh mục & Lịch trực Bác sĩ";
            this.Size = new Size(750, 450);

            lblTitle = new Label()
            {
                Text = "LỊCH TRỰC VÀ DANH MỤC BÁC SĨ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 15),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            dgvDoctors = new DataGridView()
            {
                Location = new Point(20, 50),
                Size = new Size(690, 330),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvDoctors.Columns.Add("DoctorId", "Mã BS");
            dgvDoctors.Columns.Add("DoctorName", "Tên Bác sĩ");
            dgvDoctors.Columns.Add("Department", "Khoa / Phòng");
            dgvDoctors.Columns.Add("Shift", "Ca trực");

            // Thêm dữ liệu mẫu
            dgvDoctors.Rows.Add("BS01", "BS. Nguyễn Văn A", "Khoa Cấp cứu", "Sáng (07:00 - 15:00)");
            dgvDoctors.Rows.Add("BS02", "BS. Hoàng Thị B", "Khoa Nội", "Chiều (15:00 - 23:00)");
            dgvDoctors.Rows.Add("BS03", "BS. Đỗ Minh C", "Khoa Ngoại", "Đêm (23:00 - 07:00)");

            this.Controls.Add(dgvDoctors);
        }
    }
}
