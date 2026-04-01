using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using QuanLyBanHang.Data;

namespace QuanLyBanHang.Reports
{
    public partial class FrmThongKeDoanhThu : Form
    {
        QLBHDbContext context = new QLBHDbContext();
        string reportsFolder = Application.StartupPath.Replace("bin\\Debug\\net8.0-windows", "Reports");

        public FrmThongKeDoanhThu()
        {
            InitializeComponent();

            // THÊM DÒNG NÀY ĐỂ TRÁNH LỖI TRẮNG FORM NHƯ BÀI TRƯỚC:
            this.Controls.Add(reportViewer1);
        }

        private void FrmThongKeDoanhThu_Load(object sender, EventArgs e)
        {
            // BƯỚC 1: Lấy dữ liệu thô từ Database lên C# trước (Chưa vội tính tổng để tránh lỗi EF Core)
            var rawData = context.HoaDon.Select(hd => new
            {
                MaHD = hd.ID,
                NgayLap = hd.NgayLap,
                KhachHang = hd.KhachHang.HoVaTen,
                NhanVien = hd.NhanVien.HoVaTen,
                // Chỉ lấy danh sách số lượng và đơn giá của từng chi tiết
                ChiTiet = hd.HoaDon_ChiTiet.Select(ct => new { ct.SoLuongBan, ct.DonGiaBan })
            }).ToList(); // Lệnh ToList() này sẽ ngắt câu lệnh SQL tại đây và đưa dữ liệu vào RAM

            // BƯỚC 2: Dùng C# để tính tổng tiền (C# tính toán Int16 cực kỳ an toàn)
            var danhSachDoanhThu = rawData.Select(item => new
            {
                MaHD = item.MaHD,
                NgayLap = item.NgayLap,
                KhachHang = item.KhachHang,
                NhanVien = item.NhanVien,
                TongTien = item.ChiTiet.Sum(ct => (int)ct.SoLuongBan * ct.DonGiaBan) // Tính tổng mượt mà
            }).ToList();

            // BƯỚC 3: Đưa dữ liệu vào bảng tạm của DataSet
            QLBHDataSet.DanhSachDoanhThuDataTable dtDoanhThu = new QLBHDataSet.DanhSachDoanhThuDataTable();
            dtDoanhThu.Clear();

            foreach (var item in danhSachDoanhThu)
            {
                dtDoanhThu.AddDanhSachDoanhThuRow(
                    item.MaHD,
                    item.NgayLap,
                    item.KhachHang,
                    item.NhanVien,
                    item.TongTien
                );
            }

            // BƯỚC 4: Đổ dữ liệu lên ReportViewer
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Name = "DanhSachDoanhThu"; // Tên này phải trùng với tên Dataset ở RDLC
            reportDataSource.Value = dtDoanhThu;

            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(reportDataSource);

            // Trỏ đường dẫn tới file Report Doanh Thu
            reportViewer1.LocalReport.ReportPath = Path.Combine(reportsFolder, "rptThongKeDoanhThu.rdlc");

            // Hiển thị ở chế độ in
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;
            reportViewer1.RefreshReport();
        }
    }
}