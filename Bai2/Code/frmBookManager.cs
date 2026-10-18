using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BookManagerApp
{
    public partial class frmBookManager : Form
    {
        // Khai báo các thành phần giao diện
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusCount, lblStatusTotalValue, lblStatusTime;
        private Timer timer;

        private Label lblBookId, lblTitle, lblAuthor, lblPrice, lblQuantity, lblSearch;
        private TextBox txtBookId, txtTitle, txtAuthor, txtPrice, txtQuantity, txtSearch;
        private PictureBox picBookCover;
        private Button btnLoadCover, btnAdd, btnUpdate, btnDelete, btnClear, btnSearch;
        private DataGridView dgvBooks;
        private ErrorProvider errorProvider;

        // Quản lý dữ liệu đồng bộ
        private BindingList<BookModel> bookList;
        private BindingSource bindingSource;
        private int autoId = 1;

        public frmBookManager()
        {
            InitializeComponent();
            SetupCustomComponents();
            LoadSampleData();
        }

        private void SetupCustomComponents()
        {
            // Cấu hình Form chính
            this.Text = "Hệ thống Quản lý Tài liệu Sách & Nhà sách";
            this.Size = new Size(1000, 720);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. MenuStrip[cite: 5]
            menuStrip = new MenuStrip();

            ToolStripMenuItem menuFile = new ToolStripMenuItem("File");
            ToolStripMenuItem mnuOpenCsv = new ToolStripMenuItem("Mở file CSV", null, MnuOpenCsv_Click);
            ToolStripMenuItem mnuSaveCsv = new ToolStripMenuItem("Lưu file CSV", null, MnuSaveCsv_Click);
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Thoát", null, (s, e) => this.Close());
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuOpenCsv, mnuSaveCsv, new ToolStripSeparator(), mnuExit });

            ToolStripMenuItem menuEdit = new ToolStripMenuItem("Edit");
            ToolStripMenuItem mnuAdd = new ToolStripMenuItem("Thêm mới", null, BtnAdd_Click);
            ToolStripMenuItem mnuDelete = new ToolStripMenuItem("Xóa sách", null, BtnDelete_Click);
            menuEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuAdd, mnuDelete });

            menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuEdit });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 2. ToolStrip[cite: 5]
            toolStrip = new ToolStrip();
            ToolStripButton tsbOpen = new ToolStripButton("Mở file", null, MnuOpenCsv_Click);
            ToolStripButton tsbSave = new ToolStripButton("Lưu file", null, MnuSaveCsv_Click);
            ToolStripButton tsbAdd = new ToolStripButton("Thêm mới", null, BtnAdd_Click);
            toolStrip.Items.AddRange(new ToolStripItem[] { tsbOpen, tsbSave, new ToolStripSeparator(), tsbAdd });
            this.Controls.Add(toolStrip);

            // 3. StatusStrip[cite: 5]
            statusStrip = new StatusStrip();
            lblStatusCount = new ToolStripStatusLabel("Tổng số lượng sách: 0 | ");
            lblStatusTotalValue = new ToolStripStatusLabel("Tổng giá trị kho: 0 VNĐ | ");
            lblStatusTime = new ToolStripStatusLabel();
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusCount, lblStatusTotalValue, lblStatusTime });
            this.Controls.Add(statusStrip);

            // Timer cập nhật thời gian thực
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => { lblStatusTime.Text = $"Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"; };
            timer.Start();

            // 4. Khu vực chi tiết thông tin sách & PictureBox[cite: 5, 6]
            int startY = 70;
            lblBookId = new Label() { Text = "Mã sách:", Location = new Point(30, startY), AutoSize = true };
            txtBookId = new TextBox() { Location = new Point(130, startY - 3), Width = 180, ReadOnly = true };

            lblTitle = new Label() { Text = "Tên sách:", Location = new Point(30, startY + 40), AutoSize = true };
            txtTitle = new TextBox() { Location = new Point(130, startY + 37), Width = 180 };

            lblAuthor = new Label() { Text = "Tác giả:", Location = new Point(30, startY + 80), AutoSize = true };
            txtAuthor = new TextBox() { Location = new Point(130, startY + 77), Width = 180 };

            lblPrice = new Label() { Text = "Đơn giá:", Location = new Point(30, startY + 120), AutoSize = true };
            txtPrice = new TextBox() { Location = new Point(130, startY + 117), Width = 180 };

            lblQuantity = new Label() { Text = "Số lượng:", Location = new Point(30, startY + 160), AutoSize = true };
            txtQuantity = new TextBox() { Location = new Point(130, startY + 157), Width = 180 };

            picBookCover = new PictureBox()
            {
                Location = new Point(350, startY),
                Size = new Size(130, 160),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            btnLoadCover = new Button() { Text = "Tải ảnh bìa", Location = new Point(350, startY + 165), Width = 130, Height = 30 };
            btnLoadCover.Click += BtnLoadCover_Click;

            lblSearch = new Label() { Text = "Tìm kiếm:", Location = new Point(530, startY), AutoSize = true };
            txtSearch = new TextBox() { Location = new Point(610, startY - 3), Width = 180 };
            btnSearch = new Button() { Text = "Tìm", Location = new Point(800, startY - 5), Width = 70, Height = 28 };
            btnSearch.Click += BtnSearch_Click;

            this.Controls.AddRange(new Control[] {
                lblBookId, txtBookId, lblTitle, txtTitle, lblAuthor, txtAuthor,
                lblPrice, txtPrice, lblQuantity, txtQuantity, picBookCover, btnLoadCover,
                lblSearch, txtSearch, btnSearch
            });

            // 5. Khu vực chức năng (Buttons)
            int btnY = startY + 210;
            btnAdd = new Button() { Text = "Thêm mới", Location = new Point(30, btnY), Width = 100, Height = 35 };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button() { Text = "Sửa", Location = new Point(140, btnY), Width = 100, Height = 35 };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button() { Text = "Xóa", Location = new Point(250, btnY), Width = 100, Height = 35 };
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button() { Text = "Làm mới", Location = new Point(360, btnY), Width = 100, Height = 35 };
            btnClear.Click += BtnClear_Click;

            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            // 6. DataGridView[cite: 5, 6]
            dgvBooks = new DataGridView()
            {
                Location = new Point(30, btnY + 50),
                Size = new Size(915, 290),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false
            };

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookId", HeaderText = "Mã sách", Width = 90 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tên sách", Width = 220 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Author", HeaderText = "Tác giả", Width = 160 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Đơn giá", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng", Width = 90 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CoverImagePath", HeaderText = "Ảnh bìa", Width = 210 });

            dgvBooks.SelectionChanged += DgvBooks_SelectionChanged;
            this.Controls.Add(dgvBooks);

            errorProvider = new ErrorProvider();
            txtBookId.Text = $"B{autoId:D3}";
        }

        private void LoadSampleData()
        {
            bookList = new BindingList<BookModel>();
            bindingSource = new BindingSource(bookList, null);
            dgvBooks.DataSource = bindingSource;

            bookList.Add(new BookModel { BookId = $"B{autoId++:D3}", Title = "Lập trình C# cơ bản", Author = "Nguyen Van A", Price = 120000, Quantity = 15, CoverImagePath = "" });
            bookList.Add(new BookModel { BookId = $"B{autoId++:D3}", Title = "Cấu trúc dữ liệu và giải thuật", Author = "Tran Van B", Price = 180000, Quantity = 10, CoverImagePath = "" });

            UpdateStatusInfo();
            txtBookId.Text = $"B{autoId:D3}";
        }

        private void UpdateStatusInfo()
        {
            int totalCount = 0;
            decimal totalValue = 0;
            foreach (var item in bookList)
            {
                totalCount += item.Quantity;
                totalValue += item.Price * item.Quantity;
            }
            lblStatusCount.Text = $"Tổng số lượng sách: {totalCount} | ";
            lblStatusTotalValue.Text = $"Tổng giá trị kho: {totalValue:N0} VNĐ | ";
        }

        private void BtnLoadCover_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    picBookCover.Image = Image.FromFile(ofd.FileName);
                    picBookCover.Tag = ofd.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow != null && dgvBooks.CurrentRow.DataBoundItem is BookModel selectedBook)
            {
                txtBookId.Text = selectedBook.BookId;
                txtTitle.Text = selectedBook.Title;
                txtAuthor.Text = selectedBook.Author;
                txtPrice.Text = selectedBook.Price.ToString("G0");
                txtQuantity.Text = selectedBook.Quantity.ToString();

                if (!string.IsNullOrEmpty(selectedBook.CoverImagePath) && File.Exists(selectedBook.CoverImagePath))
                {
                    picBookCover.Image = Image.FromFile(selectedBook.CoverImagePath);
                    picBookCover.Tag = selectedBook.CoverImagePath;
                }
                else
                {
                    picBookCover.Image = null;
                    picBookCover.Tag = "";
                }
            }
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                errorProvider.SetError(txtTitle, "Tên sách không được để trống!");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                errorProvider.SetError(txtAuthor, "Tác giả không được để trống!");
                isValid = false;
            }
            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price <= 0)
            {
                errorProvider.SetError(txtPrice, "Đơn giá phải là số lớn hơn 0!");
                isValid = false;
            }
            if (!int.TryParse(txtQuantity.Text.Trim(), out int qty) || qty < 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng phải là số nguyên không âm!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            BookModel newBook = new BookModel()
            {
                BookId = $"B{autoId++:D3}",
                Title = txtTitle.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Price = decimal.Parse(txtPrice.Text.Trim()),
                Quantity = int.Parse(txtQuantity.Text.Trim()),
                CoverImagePath = picBookCover.Tag != null ? picBookCover.Tag.ToString() : ""
            };

            bookList.Add(newBook);
            UpdateStatusInfo();
            BtnClear_Click(sender, e);
            MessageBox.Show("Thêm sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn sách cần sửa trên lưới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            if (dgvBooks.CurrentRow.DataBoundItem is BookModel currentBook)
            {
                currentBook.Title = txtTitle.Text.Trim();
                currentBook.Author = txtAuthor.Text.Trim();
                currentBook.Price = decimal.Parse(txtPrice.Text.Trim());
                currentBook.Quantity = int.Parse(txtQuantity.Text.Trim());
                currentBook.CoverImagePath = picBookCover.Tag != null ? picBookCover.Tag.ToString() : "";

                bindingSource.ResetBindings(false);
                UpdateStatusInfo();
                MessageBox.Show("Cập nhật sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn sách cần xóa trên lưới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dr = MessageBox.Show("Bạn có chắc chắn muốn xóa đầu sách này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                if (dgvBooks.CurrentRow.DataBoundItem is BookModel currentBook)
                {
                    bookList.Remove(currentBook);
                    UpdateStatusInfo();
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Vui lòng nhập từ khóa cần tìm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool found = false;
            foreach (DataGridViewRow row in dgvBooks.Rows)
            {
                if (row.DataBoundItem is BookModel book)
                {
                    if (book.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        book.Author.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        row.Selected = true;
                        dgvBooks.CurrentCell = row.Cells[0];
                        found = true;
                        break;
                    }
                }
            }
            if (!found) MessageBox.Show("Không tìm thấy sách phù hợp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MnuSaveCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV file (*.csv)|*.csv";
            sfd.FileName = "BookExport.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("BookId,Title,Author,Price,Quantity,CoverImagePath");
                        foreach (var book in bookList)
                        {
                            sw.WriteLine($"{book.BookId},{book.Title},{book.Author},{book.Price},{book.Quantity},{book.CoverImagePath}");
                        }
                    }
                    MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MnuOpenCsv_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV file (*.csv)|*.csv";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(ofd.FileName, System.Text.Encoding.UTF8);
                    bookList.Clear();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] parts = lines[i].Split(',');
                        if (parts.Length >= 6)
                        {
                            bookList.Add(new BookModel
                            {
                                BookId = parts[0],
                                Title = parts[1],
                                Author = parts[2],
                                Price = decimal.Parse(parts[3]),
                                Quantity = int.Parse(parts[4]),
                                CoverImagePath = parts[5]
                            });
                        }
                    }
                    UpdateStatusInfo();
                    MessageBox.Show("Nhập file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtBookId.Text = $"B{autoId:D3}";
            txtTitle.Clear();
            txtAuthor.Clear();
            txtPrice.Clear();
            txtQuantity.Clear();
            txtSearch.Clear();
            picBookCover.Image = null;
            picBookCover.Tag = "";
            errorProvider.Clear();
            dgvBooks.ClearSelection();
        }
    }
}
