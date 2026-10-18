using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevenueAnalyticsApp
{
    public partial class frmRevenueAnalytics : Form
    {
        // 1. Khai báo các Control giao diện
        private Label lblFromDate, lblToDate, lblBranch, lblProcessingStatus, lblTitle;
        private DateTimePicker dtpFromDate, dtpToDate;
        private ComboBox cboBranch;
        private Button btnAnalytics, btnCancel;
        private ProgressBar pgbLoading;
        private DataGridView dgvRevenueData;

        // Quản lý đa luồng & hủy tác vụ
        private CancellationTokenSource cts;
        private BindingList<RevenueModel> revenueList;
        private BindingSource bindingSource;

        public frmRevenueAnalytics()
        {
            InitializeComponent();
            SetupCustomComponents();
        }

        private void SetupCustomComponents()
        {
            // Cấu hình Form
            this.Text = "Bảng điều khiển Thống kê Doanh thu Multi-Threading";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tiêu đề
            lblTitle = new Label()
            {
                Text = "BẢNG ĐIỀU KHIỂN THỐNG KÊ DOANH THU",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(250, 15),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 2. Khu vực tiêu chí lọc (DateTimePicker, ComboBox, Buttons)
            int startY = 60;
            lblFromDate = new Label() { Text = "Từ ngày:", Location = new Point(30, startY), AutoSize = true };
            dtpFromDate = new DateTimePicker() { Location = new Point(100, startY - 3), Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };

            lblToDate = new Label() { Text = "Đến ngày:", Location = new Point(260, startY), AutoSize = true };
            dtpToDate = new DateTimePicker() { Location = new Point(330, startY - 3), Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Now };

            lblBranch = new Label() { Text = "Chi nhánh:", Location = new Point(490, startY), AutoSize = true };
            cboBranch = new ComboBox() { Location = new Point(570, startY - 3), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cboBranch.Items.AddRange(new string[] { "Tất cả chi nhánh", "Hà Nội", "TP. Hồ Chí Minh", "Đà Nẵng" });
            cboBranch.SelectedIndex = 0;

            btnAnalytics = new Button() { Text = "Thống Kê Doanh Thu", Location = new Point(750, startY - 5), Width = 150, Height = 32, BackColor = Color.LightSkyBlue };
            btnCancel = new Button() { Text = "Hủy Thao Tác", Location = new Point(750, startY + 35), Width = 150, Height = 32, Enabled = false, BackColor = Color.LightPink };

            btnAnalytics.Click += BtnAnalytics_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] {
                lblFromDate, dtpFromDate, lblToDate, dtpToDate, lblBranch, cboBranch, btnAnalytics, btnCancel
            });

            // 3. Khu vực trạng thái tiến trình (ProgressBar, Label)
            int statusY = startY + 75;
            lblProcessingStatus = new Label()
            {
                Text = "Trạng thái: Sẵn sàng thực hiện",
                Location = new Point(30, statusY),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Italic),
                ForeColor = Color.DarkSlateGray
            };

            pgbLoading = new ProgressBar()
            {
                Location = new Point(30, statusY + 25),
                Size = new Size(870, 23),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            this.Controls.AddRange(new Control[] { lblProcessingStatus, pgbLoading });

            // 4. Khu vực kết quả (DataGridView dgvRevenueData)
            dgvRevenueData = new DataGridView()
            {
                Location = new Point(30, statusY + 60),
                Size = new Size(870, 380),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TransactionId", HeaderText = "Mã Giao Dịch", Width = 130 });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TransactionDate", HeaderText = "Ngày GD", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BranchName", HeaderText = "Chi Nhánh", Width = 160 });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Khách Hàng", Width = 200 });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Amount", HeaderText = "Doanh Thu (VNĐ)", Width = 180, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });

            this.Controls.Add(dgvRevenueData);

            revenueList = new BindingList<RevenueModel>();
            bindingSource = new BindingSource(revenueList, null);
            dgvRevenueData.DataSource = bindingSource;
        }

        // Xử lý Bất đồng bộ (Task.Run & async/await)
        private async void BtnAnalytics_Click(object sender, EventArgs e)
        {
            // Thiết lập trạng thái Control
            btnAnalytics.Enabled = false;
            btnCancel.Enabled = true;
            pgbLoading.Value = 0;
            revenueList.Clear();
            lblProcessingStatus.Text = "Đang kết nối CSDL và tổng hợp dữ liệu doanh thu...";

            // Khởi tạo CancellationTokenSource để quản lý việc hủy tiến trình
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Khai báo Progress<T> để cập nhật giao diện an toàn từ Background Thread xuống UI Thread
            var progress = new Progress<int>(percent =>
            {
                pgbLoading.Value = percent;
                lblProcessingStatus.Text = $"Đang xử lý dữ liệu lớn... ({percent}%)";
            });

            try
            {
                string selectedBranch = cboBranch.SelectedItem.ToString();

                // Đẩy công việc tổng hợp dữ liệu xuống Background Thread bằng Task.Run[cite: 10, 11]
                List<RevenueModel> resultData = await Task.Run(() => FetchAndProcessRevenueData(selectedBranch, progress, token), token);

                // Cập nhật kết quả lên UI sau khi hoàn tất mà không làm đơ giao diện[cite: 10, 11]
                foreach (var item in resultData)
                {
                    revenueList.Add(item);
                }

                lblProcessingStatus.Text = $"Hoàn tất thống kê! Tổng số giao dịch: {resultData.Count}";
                MessageBox.Show("Tổng hợp và thống kê doanh thu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                // Xử lý khi người dùng nhấn Hủy Thao Tác
                pgbLoading.Value = 0;
                lblProcessingStatus.Text = "Đã hủy thao tác thống kê doanh thu!";
                MessageBox.Show("Đã hủy thao tác thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình tính toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Khôi phục trạng thái nút bấm
                btnAnalytics.Enabled = true;
                btnCancel.Enabled = false;
                if (cts != null)
                {
                    cts.Dispose();
                    cts = null;
                }
            }
        }

        // Giả lập tiến trình tính toán dữ liệu lớn chạy từ 4-6 giây trên Background Thread[cite: 10, 11]
        private List<RevenueModel> FetchAndProcessRevenueData(string branch, IProgress<int> progress, CancellationToken token)
        {
            List<RevenueModel> data = new List<RevenueModel>();
            Random rand = new Random();
            string[] branches = { "Hà Nội", "TP. Hồ Chí Minh", "Đà Nẵng" };
            string[] customers = { "Công ty A", "Tập đoàn B", "Nguyễn Văn C", "Trần Thị D", "Lê Văn E" };

            // Giả lập 5 bước xử lý tốn tổng cộng khoảng 5 giây (mỗi bước 1 giây)
            for (int step = 1; step <= 5; step++)
            {
                // Kiểm tra yêu cầu hủy thao tác
                token.ThrowIfCancellationRequested();

                // Dùng Thread.Sleep hoặc Task.Delay giả lập tính toán nặng trong 1000ms[cite: 10, 11]
                Thread.Sleep(1000);

                // Báo tiến độ về cho UI Thread
                int percentComplete = step * 20;
                progress?.Report(percentComplete);
            }

            // Sinh dữ liệu giả lập kết quả
            int totalRecords = rand.Next(15, 30);
            for (int i = 1; i <= totalRecords; i++)
            {
                string itemBranch = (branch == "Tất cả chi nhánh") ? branches[rand.Next(branches.Length)] : branch;
                data.Add(new RevenueModel
                {
                    TransactionId = $"GD{1000 + i}",
                    TransactionDate = DateTime.Now.AddDays(-rand.Next(1, 30)),
                    BranchName = itemBranch,
                    CustomerName = customers[rand.Next(customers.Length)],
                    Amount = rand.Next(100, 5000) * 100000m
                });
            }

            return data;
        }

        // Nút Hủy Thao Tác (Cancellation)[cite: 10, 11]
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                lblProcessingStatus.Text = "Đang gửi yêu cầu hủy thao tác...";
                cts.Cancel(); // Ngắt tiến trình ngầm an toàn bằng CancellationTokenSource[cite: 11]
            }
        }
    }
}
