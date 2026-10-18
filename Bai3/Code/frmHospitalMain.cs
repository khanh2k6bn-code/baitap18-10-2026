using System;
using System.Drawing;
using System.Windows.Forms;

namespace HospitalManagerApp
{
    public partial class frmHospitalMain : Form
    {
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblDoctorInfo;
        private ToolStripProgressBar progressBar;

        public frmHospitalMain()
        {
            InitializeComponent();
            SetupMdiMainForm();
        }

        private void SetupMdiMainForm()
        {
            // 1. Cấu hình Form cha MDI
            this.Text = "Hệ thống MDI Quản lý Bệnh viện & Truy xuất Hồ sơ Bệnh án";
            this.IsMdiContainer = true; // Thuộc tính IsMdiContainer = true theo đề bài
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 2. Tạo MenuStrip điều hướng
            menuStrip = new MenuStrip();

            // Menu Hệ thống
            ToolStripMenuItem mnuSystem = new ToolStripMenuItem("Hệ thống");
            ToolStripMenuItem mnuLogout = new ToolStripMenuItem("Đăng xuất", null, (s, e) => MessageBox.Show("Đã đăng xuất hệ thống!", "Thông báo"));
            ToolStripMenuItem mnuReload = new ToolStripMenuItem("Nạp lại dữ liệu", null, (s, e) => MessageBox.Show("Đã nạp lại dữ liệu!", "Thông báo"));
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Thoát", null, (s, e) => Application.Exit());
            mnuSystem.DropDownItems.AddRange(new ToolStripItem[] { mnuLogout, mnuReload, new ToolStripSeparator(), mnuExit });

            // Menu Quản lý
            ToolStripMenuItem mnuManage = new ToolStripMenuItem("Quản lý");
            ToolStripMenuItem mnuPatients = new ToolStripMenuItem("Hồ sơ Bệnh nhân", null, MnuPatients_Click);
            ToolStripMenuItem mnuDoctors = new ToolStripMenuItem("Danh mục Bác sĩ", null, MnuDoctors_Click);
            ToolStripMenuItem mnuDepts = new ToolStripMenuItem("Danh mục Khoa/Phòng", null, (s, e) => MessageBox.Show("Chức năng Danh mục Khoa/Phòng đang phát triển!"));
            mnuManage.DropDownItems.AddRange(new ToolStripItem[] { mnuPatients, mnuDoctors, mnuDepts });

            // Menu Cửa sổ (Sắp xếp MDI Forms)
            ToolStripMenuItem mnuWindow = new ToolStripMenuItem("Cửa sổ");
            ToolStripMenuItem mnuCascade = new ToolStripMenuItem("Xếp tầng (Cascade)", null, (s, e) => this.LayoutMdi(MdiLayout.Cascade));
            ToolStripMenuItem mnuTileHorizontal = new ToolStripMenuItem("Ngang (Tile Horizontal)", null, (s, e) => this.LayoutMdi(MdiLayout.TileHorizontal));
            ToolStripMenuItem mnuTileVertical = new ToolStripMenuItem("Dọc (Tile Vertical)", null, (s, e) => this.LayoutMdi(MdiLayout.TileVertical));
            ToolStripMenuItem mnuCloseAll = new ToolStripMenuItem("Đóng tất cả", null, MnuCloseAll_Click);
            mnuWindow.DropDownItems.AddRange(new ToolStripItem[] { mnuCascade, mnuTileHorizontal, mnuTileVertical, new ToolStripSeparator(), mnuCloseAll });

            menuStrip.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuManage, mnuWindow });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 3. StatusStrip
            statusStrip = new StatusStrip();
            lblDoctorInfo = new ToolStripStatusLabel("Bác sĩ trực: BS. Nguyễn Văn A (Khoa Cấp cứu) | ");
            progressBar = new ToolStripProgressBar() { Value = 100, Width = 150 };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblDoctorInfo, progressBar });
            this.Controls.Add(statusStrip);
        }

        // Quản lý Mở Form con Single Instance (Ngăn chặn mở lặp lại Duplicate)
        private void OpenChildForm<T>() where T : Form, new()
        {
            // Duyệt qua danh sách MdiChildren
            foreach (Form child in this.MdiChildren)
            {
                if (child is T)
                {
                    // Nếu Form đã mở, gọi lệnh Activate() để mang Form đó lên trên cùng
                    child.Activate();
                    return;
                }
            }

            // Nếu Form chưa mở, khởi tạo Instance mới, gán MdiParent = this và gọi .Show()
            T newChild = new T
            {
                MdiParent = this
            };
            newChild.Show();
        }

        private void MnuPatients_Click(object sender, EventArgs e)
        {
            OpenChildForm<frmPatientProfiles>();
        }

        private void MnuDoctors_Click(object sender, EventArgs e)
        {
            OpenChildForm<frmDoctorSchedule>();
        }

        private void MnuCloseAll_Click(object sender, EventArgs e)
        {
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }
        }
    }
}
