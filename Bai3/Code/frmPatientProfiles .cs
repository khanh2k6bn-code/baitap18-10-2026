using System;
using System.Drawing;
using System.Windows.Forms;

namespace HospitalManagerApp
{
    public partial class frmPatientProfiles : Form
    {
        private DataGridView dgvPatients;
        private Label lblTitle;

        public frmPatientProfiles()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Hồ sơ Bệnh nhân";
            this.Size = new Size(750, 450);

            lblTitle = new Label()
            {
                Text = "DANH SÁCH HỒ SƠ BỆNH NHÂN",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(20, 15),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            dgvPatients = new DataGridView()
            {
                Location = new Point(20, 50),
                Size = new Size(690, 330),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvPatients.Columns.Add("PatientId", "Mã BN");
            dgvPatients.Columns.Add("FullName", "Họ và tên");
            dgvPatients.Columns.Add("Gender", "Giới tính");
            dgvPatients.Columns.Add("Age", "Tuổi");
            dgvPatients.Columns.Add("Diagnosis", "Chẩn đoán");

            // Thêm dữ liệu mẫu
            dgvPatients.Rows.Add("BN001", "Trần Văn Hùng", "Nam", 45, "Viêm ruột thừa cấp");
            dgvPatients.Rows.Add("BN002", "Lê Thị Mai", "Nữ", 32, "Sốt xuất huyết");
            dgvPatients.Rows.Add("BN003", "Phạm Quốc Bảo", "Nam", 60, "Tăng huyết áp");

            this.Controls.Add(dgvPatients);
        }
    }
}
