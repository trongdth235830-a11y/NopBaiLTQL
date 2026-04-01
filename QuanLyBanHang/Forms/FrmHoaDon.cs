using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using QuanLyBanHang.Data;
using System.Drawing.Printing;

namespace QuanLyBanHang.Forms
{
    public partial class FrmHoaDon : Form
    {
        public FrmHoaDon()
        {
            InitializeComponent();
        }
        QLBHDbContext context = new QLBHDbContext();    // Khởi tạo biến ngữ cảnh CSDL 
        int id;

        private void FrmHoaDon_Load(object sender, EventArgs e)
        {
            LoadDataHoaDon();
        }

        private void LoadDataHoaDon()
        {
            // Khởi tạo lại DbContext để đảm bảo luôn lấy dữ liệu mới nhất từ SQL Server
            context = new QLBHDbContext();

            dgvHoaDon.AutoGenerateColumns = false;

            List<DanhSachHoaDon> hd = new List<DanhSachHoaDon>();
            hd = context.HoaDon.Select(r => new DanhSachHoaDon
            {
                ID = r.ID,
                NhanVienID = r.NhanVienID,
                HoVaTenNhanVien = r.NhanVien.HoVaTen,
                KhachHangID = r.KhachHangID,
                HoVaTenKhachHang = r.KhachHang.HoVaTen,
                NgayLap = r.NgayLap,
                GhiChuHoaDon = r.GhiChuHoaDon,
                TongTienHoaDon = r.HoaDon_ChiTiet.Sum(x => (double)x.SoLuongBan * x.DonGiaBan),
                XemChiTiet = "Xem chi tiết"
            }).ToList();

            dgvHoaDon.DataSource = hd;
        }
        private void btnLapHoaDon_Click(object sender, EventArgs e)
        {
            using (FrmHoaDon_ChiTiet frmHoaDon_ChiTiet = new FrmHoaDon_ChiTiet())
            {
                // Code sẽ tạm dừng ở đây chờ bạn thao tác bên form Chi tiết
                frmHoaDon_ChiTiet.ShowDialog();

                // Ngay sau khi form Chi tiết đóng lại, dòng này sẽ chạy để làm mới DataGridView
                LoadDataHoaDon();
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvHoaDon.CurrentRow != null)
            {
                id = Convert.ToInt32(dgvHoaDon.CurrentRow.Cells["cotID"].Value.ToString());
                using (FrmHoaDon_ChiTiet chiTiet = new FrmHoaDon_ChiTiet(id))
                {
                    chiTiet.ShowDialog();

                    // Cập nhật lại lưới sau khi sửa xong và đóng form
                    LoadDataHoaDon();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvHoaDon.CurrentRow != null)
            {
                // Hiện MessageBox xác nhận xóa 
                var traloi = MessageBox.Show("Bạn có chắc chắn muốn xóa hóa đơn này không? Dữ liệu chi tiết hóa đơn cũng sẽ bị xóa.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // Khi xác nhận thành công thì tiến hành xóa 
                if (traloi == DialogResult.Yes)
                {
                    int maHoaDon = Convert.ToInt32(dgvHoaDon.CurrentRow.Cells["cotID"].Value.ToString());

                    // Tìm hóa đơn trong CSDL
                    var hoaDon = context.HoaDon.Find(maHoaDon);

                    if (hoaDon != null)
                    {
                        // Khi xóa hóa đơn thì cũng xóa luôn dữ liệu chi tiết hóa đơn 
                        var chiTiet = context.HoaDon_ChiTiet.Where(ct => ct.HoaDonID == maHoaDon).ToList();
                        if (chiTiet.Any())
                        {
                            context.HoaDon_ChiTiet.RemoveRange(chiTiet);
                        }

                        // Xóa hóa đơn chính
                        context.HoaDon.Remove(hoaDon);

                        // Lưu các thay đổi vào CSDL
                        context.SaveChanges();

                        MessageBox.Show("Đã xóa hóa đơn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Tải lại form để cập nhật DataGridView 
                        FrmHoaDon_Load(sender, e);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            var traloi = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (traloi == DialogResult.Yes)
                this.Close();
        }

        private void dgvHoaDon_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra xem người dùng có click vào đúng cột "XemChiTiet" và không click vào tiêu đề cột (RowIndex >= 0)
            if (e.RowIndex >= 0 && dgvHoaDon.Columns[e.ColumnIndex].Name == "XemChiTiet")
            {
                // Lấy ID hóa đơn từ dòng được click
                int maHoaDon = Convert.ToInt32(dgvHoaDon.Rows[e.RowIndex].Cells["cotID"].Value.ToString());

                // Mở form chi tiết và truyền ID sang (giống như chức năng Sửa)
                using (FrmHoaDon_ChiTiet chiTiet = new FrmHoaDon_ChiTiet(maHoaDon, true))
                {
                    chiTiet.ShowDialog();

                    // Tải lại dữ liệu sau khi đóng form chi tiết (nếu bạn có chỉnh sửa gì đó bên trong)
                    LoadDataHoaDon();
                }
            }
        }

        private void btnXuat_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Xuất dữ liệu ra tập tin Excel";
            saveFileDialog.Filter = "Tập tin Excel|*.xls;*.xlsx";
            saveFileDialog.FileName = "HoaDon_" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        // ================= SHEET 1: HÓA ĐƠN =================
                        DataTable dtHoaDon = new DataTable("HoaDon");
                        dtHoaDon.Columns.AddRange(new DataColumn[] {
                    new DataColumn("ID", typeof(int)),
                    new DataColumn("NhanVienID", typeof(int)),
                    new DataColumn("KhachHangID", typeof(int)),
                    new DataColumn("NgayLap", typeof(DateTime)),
                    new DataColumn("GhiChuHoaDon", typeof(string))
                });

                        var lstHoaDon = context.HoaDon.ToList();
                        foreach (var hd in lstHoaDon)
                        {
                            dtHoaDon.Rows.Add(hd.ID, hd.NhanVienID, hd.KhachHangID, hd.NgayLap, hd.GhiChuHoaDon);
                        }
                        var sheet1 = wb.Worksheets.Add(dtHoaDon, "HoaDon");
                        sheet1.Columns().AdjustToContents();

                        // ================= SHEET 2: CHI TIẾT HÓA ĐƠN =================
                        DataTable dtChiTiet = new DataTable("HoaDon_ChiTiet");
                        dtChiTiet.Columns.AddRange(new DataColumn[] {
                    new DataColumn("ID", typeof(int)),
                    new DataColumn("HoaDonID", typeof(int)),
                    new DataColumn("SanPhamID", typeof(int)),
                    new DataColumn("SoLuongBan", typeof(short)),
                    new DataColumn("DonGiaBan", typeof(int))
                });

                        var lstChiTiet = context.HoaDon_ChiTiet.ToList();
                        foreach (var ct in lstChiTiet)
                        {
                            dtChiTiet.Rows.Add(ct.ID, ct.HoaDonID, ct.SanPhamID, ct.SoLuongBan, ct.DonGiaBan);
                        }
                        var sheet2 = wb.Worksheets.Add(dtChiTiet, "HoaDon_ChiTiet");
                        sheet2.Columns().AdjustToContents();

                        // Lưu file
                        wb.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show("Đã xuất dữ liệu ra tập tin Excel thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnNhap_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Nhập dữ liệu từ tập tin Excel";
            openFileDialog.Filter = "Tập tin Excel|*.xls;*.xlsx";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (XLWorkbook workbook = new XLWorkbook(openFileDialog.FileName))
                    {
                        // Kiểm tra xem file có đúng 2 sheet không
                        if (!workbook.Worksheets.Contains("HoaDon") || !workbook.Worksheets.Contains("HoaDon_ChiTiet"))
                        {
                            MessageBox.Show("File Excel không đúng định dạng. Cần có 2 sheet 'HoaDon' và 'HoaDon_ChiTiet'.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Dùng Dictionary để ghi nhớ: ID cũ trong Excel -> ID mới trong CSDL
                        Dictionary<int, int> mapHoaDon = new Dictionary<int, int>();

                        // --- 1. NHẬP SHEET HOADON ---
                        IXLWorksheet wsHoaDon = workbook.Worksheet("HoaDon");
                        bool firstRow = true;
                        foreach (IXLRow row in wsHoaDon.RowsUsed())
                        {
                            if (firstRow) { firstRow = false; continue; } // Bỏ qua dòng tiêu đề

                            int oldId = Convert.ToInt32(row.Cell(1).Value.ToString());

                            HoaDon hd = new HoaDon();
                            hd.NhanVienID = Convert.ToInt32(row.Cell(2).Value.ToString());
                            hd.KhachHangID = Convert.ToInt32(row.Cell(3).Value.ToString());
                            hd.NgayLap = Convert.ToDateTime(row.Cell(4).Value.ToString());
                            hd.GhiChuHoaDon = row.Cell(5).Value.ToString();

                            context.HoaDon.Add(hd);
                            context.SaveChanges(); // Lưu ngay để EF Core cấp mã ID mới

                            // Lưu ID mới vào bộ nhớ tạm
                            mapHoaDon.Add(oldId, hd.ID);
                        }

                        // --- 2. NHẬP SHEET HOADON_CHITIET ---
                        IXLWorksheet wsChiTiet = workbook.Worksheet("HoaDon_ChiTiet");
                        firstRow = true;
                        foreach (IXLRow row in wsChiTiet.RowsUsed())
                        {
                            if (firstRow) { firstRow = false; continue; }

                            int oldHoaDonId = Convert.ToInt32(row.Cell(2).Value.ToString());

                            // Nếu hóa đơn cha đã được nhập thành công thì mới nhập chi tiết
                            if (mapHoaDon.ContainsKey(oldHoaDonId))
                            {
                                HoaDon_ChiTiet ct = new HoaDon_ChiTiet();
                                ct.HoaDonID = mapHoaDon[oldHoaDonId]; // Dùng ID mới đã lưu
                                ct.SanPhamID = Convert.ToInt32(row.Cell(3).Value.ToString());
                                ct.SoLuongBan = Convert.ToInt16(row.Cell(4).Value.ToString());
                                ct.DonGiaBan = Convert.ToInt32(row.Cell(5).Value.ToString());

                                context.HoaDon_ChiTiet.Add(ct);
                            }
                        }

                        context.SaveChanges(); // Lưu tất cả chi tiết

                        MessageBox.Show("Đã nhập thành công dữ liệu hóa đơn.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDataHoaDon(); // Làm mới DataGridView
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi nhập file: Vui lòng kiểm tra lại định dạng dữ liệu trong Excel.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem người dùng đã chọn dòng nào trên DataGridView chưa
            if (dgvHoaDon.CurrentRow != null)
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
            int maHoaDon = Convert.ToInt32(dgvHoaDon.CurrentRow.Cells["cotID"].Value.ToString());

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
                g.DrawString("CỬA HÀNG THIẾT BỊ IT VÕ VĂN TỶ", fontHeader, brush, margin, y);
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
