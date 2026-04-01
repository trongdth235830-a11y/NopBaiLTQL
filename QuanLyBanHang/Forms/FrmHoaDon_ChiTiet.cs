using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyBanHang.Data;

namespace QuanLyBanHang.Forms
{

    public partial class FrmHoaDon_ChiTiet : Form
    {

        QLBHDbContext context = new QLBHDbContext();    // Khởi tạo biến ngữ cảnh CSDL 
        int id;                                         // Lấy mã hóa đơn (dùng cho Sửa và Xóa) 
        bool isViewMode;
        BindingList<DanhSachHoaDon_ChiTiet> hoaDonChiTiet = new BindingList<DanhSachHoaDon_ChiTiet>();

        private void FrmHoaDon_ChiTiet_Load(object sender, EventArgs e)
        {

            LayNhanVienVaoComboBox();
            LayKhachHangVaoComboBox();
            LaySanPhamVaoComboBox();

            dataGridView1.AutoGenerateColumns = false;
            if (id != 0) // Đã tồn tại chi tiết 
            {
                var hoaDon = context.HoaDon.Where(r => r.ID == id).SingleOrDefault();
                cboNhanVien.SelectedValue = hoaDon.NhanVienID;
                cboKhachHang.SelectedValue = hoaDon.KhachHangID;
                txtGhiChuHoaDon.Text = hoaDon.GhiChuHoaDon;

                var ct = context.HoaDon_ChiTiet.Where(r => r.HoaDonID == id).Select(r => new DanhSachHoaDon_ChiTiet
                {
                    ID = r.ID,
                    HoaDonID = r.HoaDonID,
                    SanPhamID = r.SanPhamID,
                    TenSanPham = r.SanPham.TenSanPham,
                    SoLuongBan = r.SoLuongBan,
                    DonGiaBan = r.DonGiaBan,
                    ThanhTien = Convert.ToInt32(r.SoLuongBan * r.DonGiaBan)
                }).ToList();

                hoaDonChiTiet = new BindingList<DanhSachHoaDon_ChiTiet>(ct);
            }
            if (isViewMode == true)
            {
                // Khóa thông tin chung
                cboNhanVien.Enabled = false;
                cboKhachHang.Enabled = false;
                txtGhiChuHoaDon.ReadOnly = true; // Dùng ReadOnly cho TextBox sẽ đẹp hơn Enabled

                // Đã là chế độ xem thì khóa luôn các chỗ thêm/xóa sản phẩm và nút Lưu
                cboSanPham.Enabled = false;
                numDonGiaBan.Enabled = false;
                numSoLuongBan.Enabled = false;

                btnXacNhanBan.Visible = false;
                btnXoa.Visible = false;
                btnLuuHoaDon.Visible = false;
            }

            dataGridView1.DataSource = hoaDonChiTiet;
            BatTatChucNang();
        }
        public FrmHoaDon_ChiTiet(int maHoaDon = 0, bool viewMode = false)
        {
            InitializeComponent();
            id = maHoaDon;
            isViewMode = viewMode;
        }
        public void LayNhanVienVaoComboBox()
        {
            cboNhanVien.DataSource = context.NhanVien.ToList();
            cboNhanVien.ValueMember = "ID";
            cboNhanVien.DisplayMember = "HoVaTen";
        }

        public void LayKhachHangVaoComboBox()
        {
            cboKhachHang.DataSource = context.KhachHang.ToList();
            cboKhachHang.ValueMember = "ID";
            cboKhachHang.DisplayMember = "HoVaTen";
        }
        public void LaySanPhamVaoComboBox()
        {
            cboSanPham.DataSource = context.SanPham.ToList();
            cboSanPham.ValueMember = "ID";
            cboSanPham.DisplayMember = "TenSanPham";
        }

        public void BatTatChucNang()
        {
            if (id == 0 && dataGridView1.Rows.Count == 0) // Thêm 
            {
                // Xóa trắng các trường 
                cboKhachHang.Text = "";
                cboNhanVien.Text = "";
                cboSanPham.Text = "";
                numSoLuongBan.Value = 1;
                numDonGiaBan.Value = 0;
            }

            // Nút lưu và xóa chỉ sáng khi có sản phẩm 
            btnLuuHoaDon.Enabled = dataGridView1.Rows.Count > 0;
            btnXoa.Enabled = dataGridView1.Rows.Count > 0;
        }

        private void btnXacNhanBan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboSanPham.Text))
                MessageBox.Show("Vui lòng chọn sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (numSoLuongBan.Value <= 0)
                MessageBox.Show("Số lượng bán phải lớn hơn 0.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (numDonGiaBan.Value <= 0)
                MessageBox.Show("Đơn giá bán sản phẩm phải lớn hơn 0.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            {
                int maSanPham = Convert.ToInt32(cboSanPham.SelectedValue.ToString());
                var chiTiet = hoaDonChiTiet.FirstOrDefault(x => x.SanPhamID == maSanPham);

                // Nếu đã tồn tại sản phẩm thì cập nhật thông tin  
                if (chiTiet != null)
                {
                    chiTiet.SoLuongBan = Convert.ToInt16(numSoLuongBan.Value);
                    chiTiet.DonGiaBan = Convert.ToInt32(numDonGiaBan.Value);
                    chiTiet.ThanhTien = Convert.ToInt32(numSoLuongBan.Value * numDonGiaBan.Value);
                    dataGridView1.Refresh();
                }
                else // Nếu chưa có sản phẩm thì thêm vào 
                {
                    // Nếu chưa có sản phẩm nào 
                    DanhSachHoaDon_ChiTiet ct = new DanhSachHoaDon_ChiTiet
                    {
                        ID = 0,
                        HoaDonID = id,
                        SanPhamID = maSanPham,
                        TenSanPham = cboSanPham.Text,
                        SoLuongBan = Convert.ToInt16(numSoLuongBan.Value),
                        DonGiaBan = Convert.ToInt32(numDonGiaBan.Value),
                        ThanhTien = Convert.ToInt32(numSoLuongBan.Value * numDonGiaBan.Value)
                    };
                    hoaDonChiTiet.Add(ct);
                }
                BatTatChucNang();
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            int maSanPham = Convert.ToInt32(dataGridView1.CurrentRow.Cells["SanPhamID"].Value.ToString());
            var chiTiet = hoaDonChiTiet.FirstOrDefault(x => x.SanPhamID == maSanPham);
            if (chiTiet != null)
            {
                hoaDonChiTiet.Remove(chiTiet);
            }
            BatTatChucNang();
        }

        private void btnLuuHoaDon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboNhanVien.Text))
                MessageBox.Show("Vui lòng chọn nhân viên lập hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (string.IsNullOrWhiteSpace(cboKhachHang.Text))
                MessageBox.Show("Vui lòng chọn khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                if (id != 0) // Đã tồn tại chi tiết thì chỉ cập nhật 
                {
                    HoaDon hd = context.HoaDon.Find(id);
                    if (hd != null)
                    {
                        hd.NhanVienID = Convert.ToInt32(cboNhanVien.SelectedValue.ToString());
                        hd.KhachHangID = Convert.ToInt32(cboKhachHang.SelectedValue.ToString());
                        hd.GhiChuHoaDon = txtGhiChuHoaDon.Text;
                        context.HoaDon.Update(hd);
                    }
                    // Xóa chi tiết cũ 
                    var old = context.HoaDon_ChiTiet.Where(r => r.HoaDonID == id).ToList();
                    context.HoaDon_ChiTiet.RemoveRange(old);

                    // Thêm lại chi tiết mới 
                    foreach (var item in hoaDonChiTiet.ToList())
                    {
                        HoaDon_ChiTiet ct = new HoaDon_ChiTiet();
                        ct.HoaDonID = id;
                        ct.SanPhamID = item.SanPhamID;
                        ct.SoLuongBan = item.SoLuongBan;
                        ct.DonGiaBan = item.DonGiaBan;
                        context.HoaDon_ChiTiet.Add(ct);
                    }

                    context.SaveChanges();
                }

                else // Thêm mới 
                {
                    HoaDon hd = new HoaDon();
                    hd.NhanVienID = Convert.ToInt32(cboNhanVien.SelectedValue.ToString());
                    hd.KhachHangID = Convert.ToInt32(cboKhachHang.SelectedValue.ToString());
                    hd.NgayLap = DateTime.Now;
                    hd.GhiChuHoaDon = txtGhiChuHoaDon.Text;
                    context.HoaDon.Add(hd);
                    context.SaveChanges();

                    // Thêm chi tiết 
                    foreach (var item in hoaDonChiTiet.ToList())
                    {
                        HoaDon_ChiTiet ct = new HoaDon_ChiTiet();
                        ct.HoaDonID = hd.ID;
                        ct.SanPhamID = item.SanPhamID;
                        ct.SoLuongBan = item.SoLuongBan;
                        ct.DonGiaBan = item.DonGiaBan;
                        context.HoaDon_ChiTiet.Add(ct);
                    }
                    context.SaveChanges();
                }

                MessageBox.Show("Đã lưu thành công!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cboSanPham_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int maSanPham = Convert.ToInt32(cboSanPham.SelectedValue.ToString());
            var sanPham = context.SanPham.Find(maSanPham);
            numDonGiaBan.Value = sanPham.DonGia;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            var traloi = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (traloi == DialogResult.Yes)
                this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Chọn 1 dòng trên lưới thì dữ liệu sẽ đổ lên các combobox


        }

        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
           
            if (dataGridView1.CurrentRow != null)
            {
                // Khởi tạo đối tượng in
                PrintDocument pdHoaDon = new PrintDocument();
                // Gắn sự kiện vẽ giao diện hóa đơn
                pdHoaDon.PrintPage += new PrintPageEventHandler(pdHoaDon_PrintPage);

                // Khởi tạo hộp thoại xem trước bản in (Print Preview)
                PrintPreviewDialog previewDialog = new PrintPreviewDialog();
                previewDialog.Document = pdHoaDon;

                // Mở hộp thoại xem trước ở chế độ toàn màn hình cho dễ nhìn
                ((Form)previewDialog).WindowState = FormWindowState.Maximized;
                previewDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để in.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void pdHoaDon_PrintPage(object sender, PrintPageEventArgs e)
        {
            // 1. Lấy mã hóa đơn đang được chọn trên lưới (Sử dụng cột "cotID" của bạn)
            int maHoaDon = Convert.ToInt32(dataGridView1.CurrentRow.Cells["SanPhamID"].Value.ToString());

            using (var db = new QLBHDbContext())
            {
                // 2. Lấy thông tin hóa đơn, nhân viên, khách hàng và danh sách sản phẩm
                var hoaDon = db.HoaDon.Find(maHoaDon);
                if (hoaDon == null) return;

                var khachHang = db.KhachHang.Find(hoaDon.KhachHangID);
                var nhanVien = db.NhanVien.Find(hoaDon.NhanVienID);
                var lstChiTiet = db.HoaDon_ChiTiet.Where(ct => ct.HoaDonID == maHoaDon).ToList();

                // 3. Cấu hình Font chữ và cọ vẽ (Dùng font Courier New monospaced giống máy in bill)
                Graphics g = e.Graphics;
                Font fontTitle = new Font("Courier New", 18, FontStyle.Bold);
                Font fontHeader = new Font("Courier New", 12, FontStyle.Bold);
                Font fontBody = new Font("Courier New", 10, FontStyle.Regular);
                Font fontItalic = new Font("Courier New", 10, FontStyle.Italic);
                Brush brush = Brushes.Black;

                int y = 20; // Tọa độ Y (chiều dọc) bắt đầu
                int margin = 20; // Lề trái

                // --- PHẦN HEADER (Thông tin cửa hàng) ---
                g.DrawString("CỬA HÀNG THIẾT BỊ IT Nguyễn Hữu Nhân", fontHeader, brush, margin, y);
                y += 25;
                g.DrawString("Địa chỉ: Đại học An Giang, TP. Long Xuyên", fontBody, brush, margin, y);
                y += 40;

                string title = "HÓA ĐƠN BÁN HÀNG";
                SizeF titleSize = g.MeasureString(title, fontTitle);
                g.DrawString(title, fontTitle, brush, (e.PageBounds.Width - titleSize.Width) / 2, y); // Căn giữa tiêu đề
                y += 40;

                // --- PHẦN THÔNG TIN CHUNG (Phiếu, ngày, nhân viên...) ---
                g.DrawString($"Số HĐ     : {hoaDon.ID}", fontBody, brush, margin, y);
                y += 20;
                g.DrawString($"Ngày lập  : {hoaDon.NgayLap:dd/MM/yyyy HH:mm}", fontBody, brush, margin, y);
                y += 20;
                g.DrawString($"Thu ngân  : {nhanVien?.HoVaTen}", fontBody, brush, margin, y);
                y += 20;
                g.DrawString($"Khách hàng: {khachHang?.HoVaTen}", fontBody, brush, margin, y);
                y += 30;

                // Vẽ đường gạch đứt ngang phân cách
                g.DrawString(new string('-', 85), fontBody, brush, margin, y);
                y += 20;

                // --- PHẦN TIÊU ĐỀ BẢNG SẢN PHẨM ---
                g.DrawString("Tên Sản Phẩm", fontHeader, brush, margin, y);
                g.DrawString("SL", fontHeader, brush, margin + 350, y);
                g.DrawString("Đơn Giá", fontHeader, brush, margin + 450, y);
                g.DrawString("Thành Tiền", fontHeader, brush, margin + 620, y);
                y += 25;
                g.DrawString(new string('-', 85), fontBody, brush, margin, y);
                y += 20;

                // --- PHẦN DANH SÁCH CHI TIẾT SẢN PHẨM ---
                double tongTien = 0;
                foreach (var ct in lstChiTiet)
                {
                    var sp = db.SanPham.Find(ct.SanPhamID);
                    string tenSP = sp != null ? sp.TenSanPham : "SP không xác định";

                    // Rút gọn tên SP nếu quá dài để không bị lẹm sang cột Số lượng
                    if (tenSP.Length > 35) tenSP = tenSP.Substring(0, 35) + "...";

                    double thanhTien = ct.SoLuongBan * ct.DonGiaBan;
                    tongTien += thanhTien;

                    // In từng dòng dữ liệu theo các mốc tọa độ cột tương ứng
                    g.DrawString(tenSP, fontBody, brush, margin, y);
                    g.DrawString(ct.SoLuongBan.ToString(), fontBody, brush, margin + 350, y);
                    g.DrawString(ct.DonGiaBan.ToString("N0"), fontBody, brush, margin + 450, y);
                    g.DrawString(thanhTien.ToString("N0"), fontBody, brush, margin + 620, y);
                    y += 25;
                }

                g.DrawString(new string('-', 85), fontBody, brush, margin, y);
                y += 25;

                // --- PHẦN TỔNG CỘNG ---
                g.DrawString("TỔNG CỘNG:", fontHeader, brush, margin + 450, y);
                g.DrawString(tongTien.ToString("N0") + " VNĐ", fontHeader, brush, margin + 620, y);
                y += 40;

                // --- GHI CHÚ VÀ LỜI CẢM ƠN ---
                if (!string.IsNullOrEmpty(hoaDon.GhiChuHoaDon))
                {
                    g.DrawString($"Ghi chú: {hoaDon.GhiChuHoaDon}", fontItalic, brush, margin, y);
                    y += 30;
                }

                string footer = "Cảm ơn quý khách & Hẹn gặp lại!";
                SizeF footerSize = g.MeasureString(footer, fontItalic);
                g.DrawString(footer, fontItalic, brush, (e.PageBounds.Width - footerSize.Width) / 2, y); // Căn giữa
            }
        }
    }
}
