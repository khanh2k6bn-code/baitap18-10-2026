using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GymManagerApp
{
    public partial class frmGymMemberManager : Form
    {
        // Khai báo các Control giao diện
        private Label lblTitle, lblMemberId, lblFullName, lblPhone, lblPackage, lblStartDate, lblFee;
        private TextBox txtMemberId, txtFullName, txtPhone, txtFee;
        private ComboBox cboPackage;
        private DateTimePicker dtpStartDate;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private DataGridView dgvMembers;
        private ErrorProvider errorProvider;

        // Quản lý dữ liệu đồng bộ 2 chiều
        private BindingList<MemberModel> memberList;
        private BindingSource bindingSource;
        private int autoId = 1;

        public frmGymMemberManager()
        {
            InitializeComponent();
            SetupCustomComponents();
            LoadSampleData();
        }

        private void SetupCustomComponents()
        {
            // 1. Cấu hình Form
            this.Text = "Hệ thống Quản lý Dịch vụ Phòng Gym & Hội viên";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 2. Tiêu đề
            lblTitle = new Label()
            {
                Text = "QUẢN LÝ HỘI VIÊN PHÒNG GYM",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(250, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 3. Khu vực thông tin hội viên (Labels & Controls)
            lblMemberId = new Label() { Text = "Mã hội viên:", Location = new Point(30, 70), AutoSize = true };
            txtMemberId = new TextBox() { Location = new Point(130, 67), Width = 180, ReadOnly = true };

            lblFullName = new Label() { Text = "Họ và tên:", Location = new Point(30, 110), AutoSize = true };
            txtFullName = new TextBox() { Location = new Point(130, 107), Width = 180 };

            lblPhone = new Label() { Text = "Số điện thoại:", Location = new Point(30, 150), AutoSize = true };
            txtPhone = new TextBox() { Location = new Point(130, 147), Width = 180 };

            lblPackage = new Label() { Text = "Gói tập:", Location = new Point(450, 70), AutoSize = true };
            cboPackage = new ComboBox() { Location = new Point(550, 67), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboPackage.Items.AddRange(new string[] { "Theo tháng", "6 tháng", "1 năm" });

            lblStartDate = new Label() { Text = "Ngày đăng ký:", Location = new Point(450, 110), AutoSize = true };
            dtpStartDate = new DateTimePicker() { Location = new Point(550, 107), Width = 180, Format = DateTimePickerFormat.Short };

            lblFee = new Label() { Text = "Phí dịch vụ:", Location = new Point(450, 150), AutoSize = true };
            txtFee = new TextBox() { Location = new Point(550, 147), Width = 180 };

            this.Controls.AddRange(new Control[] {
                lblMemberId, txtMemberId, lblFullName, txtFullName, lblPhone, txtPhone,
                lblPackage, cboPackage, lblStartDate, dtpStartDate, lblFee, txtFee
            });

            // 4. Khu vực chức năng (Buttons)
            btnAdd = new Button() { Text = "Thêm mới", Location = new Point(130, 200), Width = 100, Height = 35 };
            btnUpdate = new Button() { Text = "Sửa thông tin", Location = new Point(240, 200), Width = 100, Height = 35 };
            btnDelete = new Button() { Text = "Xóa hội viên", Location = new Point(350, 200), Width = 100, Height = 35 };
            btnClear = new Button() { Text = "Làm mới", Location = new Point(460, 200), Width = 100, Height = 35 };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            // 5. Khu vực lưới hiển thị DataGridView (Đã sửa sạch sẽ lỗi cú pháp ở đây)
            dgvMembers = new DataGridView()
            {
                Location = new Point(30, 260),
                Size = new Size(820, 310),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false
            };

            // Khai báo cột qua code C# và gán DataPropertyName chính xác với Class MemberModel[cite: 1, 2]
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MemberId", HeaderText = "Mã HV", Width = 90 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Tên hội viên", Width = 180 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "SĐT", Width = 110 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Package", HeaderText = "Gói tập", Width = 120 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StartDate", HeaderText = "Ngày ĐK", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });

            DataGridViewTextBoxColumn feeCol = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Fee",
                HeaderText = "Phí dịch vụ",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            };
            dgvMembers.Columns.Add(feeCol);

            dgvMembers.SelectionChanged += DgvMembers_SelectionChanged;
            this.Controls.Add(dgvMembers);

            // 6. ErrorProvider
            errorProvider = new ErrorProvider();

            // Sinh mã hội viên tự động ban đầu
            txtMemberId.Text = $"HV{autoId:D3}";
        }

        private void LoadSampleData()
        {
            memberList = new BindingList<MemberModel>();
            bindingSource = new BindingSource(memberList, null);
            dgvMembers.DataSource = bindingSource;

            // Thêm dữ liệu mẫu
            memberList.Add(new MemberModel { MemberId = $"HV{autoId++:D3}", FullName = "Nguyen Van A", Phone = "0987654321", Package = "Theo tháng", StartDate = DateTime.Now, Fee = 500000 });
            memberList.Add(new MemberModel { MemberId = $"HV{autoId++:D3}", FullName = "Tran Thi B", Phone = "0912345678", Package = "6 tháng", StartDate = DateTime.Now, Fee = 2500000 });

            txtMemberId.Text = $"HV{autoId:D3}";
        }

        private void DgvMembers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow != null && dgvMembers.CurrentRow.DataBoundItem is MemberModel selectedMember)
            {
                txtMemberId.Text = selectedMember.MemberId;
                txtFullName.Text = selectedMember.FullName;
                txtPhone.Text = selectedMember.Phone;
                cboPackage.SelectedItem = selectedMember.Package;
                dtpStartDate.Value = selectedMember.StartDate;
                txtFee.Text = selectedMember.Fee.ToString("G0");
            }
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                errorProvider.SetError(txtFullName, "Tên hội viên không được để trống!");
                isValid = false;
            }

            if (txtPhone.Text.Trim().Length != 10 || !long.TryParse(txtPhone.Text.Trim(), out _))
            {
                errorProvider.SetError(txtPhone, "Số điện thoại phải đúng 10 chữ số!");
                isValid = false;
            }

            if (!decimal.TryParse(txtFee.Text.Trim(), out decimal fee) || fee <= 0)
            {
                errorProvider.SetError(txtFee, "Phí dịch vụ phải là số nguyên dương!");
                isValid = false;
            }

            if (cboPackage.SelectedIndex == -1)
            {
                errorProvider.SetError(cboPackage, "Vui lòng chọn gói tập!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            MemberModel newMember = new MemberModel()
            {
                MemberId = $"HV{autoId++:D3}",
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Package = cboPackage.SelectedItem.ToString(),
                StartDate = dtpStartDate.Value,
                Fee = decimal.Parse(txtFee.Text.Trim())
            };

            memberList.Add(newMember);
            BtnClear_Click(sender, e);
            MessageBox.Show("Thêm hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn hội viên cần sửa trên lưới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            if (dgvMembers.CurrentRow.DataBoundItem is MemberModel currentMember)
            {
                currentMember.FullName = txtFullName.Text.Trim();
                currentMember.Phone = txtPhone.Text.Trim();
                currentMember.Package = cboPackage.SelectedItem.ToString();
                currentMember.StartDate = dtpStartDate.Value;
                currentMember.Fee = decimal.Parse(txtFee.Text.Trim());

                bindingSource.ResetBindings(false);
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn hội viên cần xóa trên lưới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dr = MessageBox.Show("Bạn có chắc chắn muốn xóa hội viên này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                if (dgvMembers.CurrentRow.DataBoundItem is MemberModel currentMember)
                {
                    memberList.Remove(currentMember);
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Đã xóa hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtMemberId.Text = $"HV{autoId:D3}";
            txtFullName.Clear();
            txtPhone.Clear();
            txtFee.Clear();
            cboPackage.SelectedIndex = -1;
            dtpStartDate.Value = DateTime.Now;
            errorProvider.Clear();
            dgvMembers.ClearSelection();
        }
    }
}
