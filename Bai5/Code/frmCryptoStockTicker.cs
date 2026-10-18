using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoTickerApp
{
    public partial class frmCryptoStockTicker : Form
    {
        // Khai báo các Control giao diện
        private Label lblTitle, lblLabelBtc, lblBTCPrice;
        private Button btnStart, btnStop;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusInfo;
        private DataGridView dgvHistoryLog;

        // Quản lý tiến trình ngầm và dữ liệu
        private CancellationTokenSource cts;
        private BindingList<TickerModel> historyList;
        private BindingSource bindingSource;
        private decimal previousPrice = 65000.00m;
        private int syncCount = 0;

        public frmCryptoStockTicker()
        {
            InitializeComponent();
            SetupCustomComponents();
        }

        private void SetupCustomComponents()
        {
            // 1. Cấu hình Form
            this.Text = "Mô phỏng Giám sát Tỷ giá Tài chính Thời gian Thực";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tiêu đề
            lblTitle = new Label()
            {
                Text = "GIÁM SÁT TỶ GIÁ BTC/USD THỜI GIAN THỰC",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(180, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 2. Khu vực điều khiển
            btnStart = new Button() { Text = "Bắt đầu Giám sát", Location = new Point(30, 65), Width = 140, Height = 35, BackColor = Color.LightGreen };
            btnStop = new Button() { Text = "Tạm dừng", Location = new Point(180, 65), Width = 110, Height = 35, Enabled = false, BackColor = Color.LightCoral };

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;

            this.Controls.AddRange(new Control[] { btnStart, btnStop });

            // 3. Khu vực hiển thị giá BTC/USD trực tiếp
            lblLabelBtc = new Label() { Text = "BTC/USD:", Location = new Point(350, 72), Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true };
            lblBTCPrice = new Label()
            {
                Text = "65,000.00 $",
                Location = new Point(440, 67),
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true
            };

            this.Controls.AddRange(new Control[] { lblLabelBtc, lblBTCPrice });

            // 4. StatusStrip hiển thị lần cập nhật cuối & số lượt đồng bộ
            statusStrip = new StatusStrip();
            lblStatusInfo = new ToolStripStatusLabel("Trạng thái: Đang dừng | Lần cập nhật cuối: --:--:-- | Số lượt đồng bộ thành công: 0");
            statusStrip.Items.Add(lblStatusInfo);
            this.Controls.Add(statusStrip);

            // 5. DataGridView ghi nhận lịch sử biến động giá (Đã sửa lỗi cú pháp Point ở đây)
            dgvHistoryLog = new DataGridView()
            {
                Location = new Point(30, 120),
                Size = new Size(775, 400),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TimeStamp", HeaderText = "Thời Gian", Width = 150 });
            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Tỷ Giá (USD)", Width = 250, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Trend", HeaderText = "Xu Hướng", Width = 330 });

            this.Controls.Add(dgvHistoryLog);

            historyList = new BindingList<TickerModel>();
            bindingSource = new BindingSource(historyList, null);
            dgvHistoryLog.DataSource = bindingSource;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            try
            {
                await Task.Run(async () =>
                {
                    Random rand = new Random();
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(2000, token);

                        decimal priceChange = (decimal)(rand.NextDouble() * 1000 - 480);
                        decimal newPrice = previousPrice + priceChange;
                        string trend = (newPrice >= previousPrice) ? "Tăng 📈" : "Giảm 📉";
                        string timeStr = DateTime.Now.ToString("HH:mm:ss");

                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                UpdateUIWithNewPrice(timeStr, newPrice, trend);
                            }));
                        }
                        else
                        {
                            UpdateUIWithNewPrice(timeStr, newPrice, trend);
                        }

                        previousPrice = newPrice;
                    }
                }, token);
            }
            catch (TaskCanceledException)
            {
                // Bắt ngoại lệ khi hủy task
            }
        }

        private void UpdateUIWithNewPrice(string timeStr, decimal price, string trend)
        {
            if (trend.Contains("Tăng"))
            {
                lblBTCPrice.ForeColor = Color.Green;
            }
            else
            {
                lblBTCPrice.ForeColor = Color.Red;
            }

            lblBTCPrice.Text = $"{price:N2} $";
            syncCount++;

            lblStatusInfo.Text = $"Trạng thái: Đang giám sát | Lần cập nhật cuối: {timeStr} | Số lượt đồng bộ thành công: {syncCount}";

            historyList.Insert(0, new TickerModel
            {
                TimeStamp = timeStr,
                Price = price,
                Trend = trend
            });
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatusInfo.Text = $"Trạng thái: Đã tạm dừng | Lần cập nhật cuối: {DateTime.Now:HH:mm:ss} | Số lượt đồng bộ thành công: {syncCount}";
        }
    }
}
