using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebAppDoCongNghe.Models.ApiRespone;
using WebAppDoCongNghe.Models.Entity;
using WebAppDoCongNghe.Models.model;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhieuGiamGiaController : ControllerBase
    {
        private readonly WebAppDoCongNgheContext _context;

        public PhieuGiamGiaController(WebAppDoCongNgheContext context)
        {
            _context = context;
        }

        [HttpGet("Trang")]
        public IActionResult AdminPGG(int page, int pageSize) 
        {
            var item = _context.PhieuGiamGia.ToList().AsQueryable();

            int total = item.Count();

            var items = item
                      .OrderByDescending(x => x.Id)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách danh mục thành công",
                data = new
                {
                    items = items,
                    total = total,
                    page = page,
                    pageSize = pageSize
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PhieuGiamGia.ToListAsync();
            return Ok(
                new ApiRespone
                {
                    Success = true,
                    Message = "lấy danh sách phiếu giảm giá thành công",
                    Data = data
                }
                );
        }

        [HttpPost]
        public async Task<IActionResult> Create(PhieuGiamGiaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ApiRespone
                    {
                        Success = false,
                        Message = " lỗi validate dữ liệu"
                    });
            }

            var PhieuGiam = new PhieuGiamGium
            {
                MaPhieu = request.MaPhieu,
                MoTa = request.MoTa,
                GiaTriGiam = request.GiaTriGiam,
                KieuGiam = request.KieuGiam,
                NgayBatDau = request.NgayBatDau,
                NgayKetThuc = request.NgayKetThuc,
                SoLuong = request.SoLuong,
                
                TrangThai = request.TrangThai
            };
            _context.PhieuGiamGia.Add(PhieuGiam);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    Success = true,
                    message = "tạo thành công phiếu giảm giá",
                    data = PhieuGiam

                });


        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PhieuGiamGiaRequest request)
        {
            var entity = await _context.PhieuGiamGia.FindAsync(id);
            if (entity == null) {
                return BadRequest(new ApiRespone { 
                 Success = false,
                 Message ="lỗi không tìm thấy phiếu giảm giá"
                });
            }

            entity.MaPhieu = request.MaPhieu;
            entity.MoTa = request.MoTa;
            entity.GiaTriGiam = request.GiaTriGiam;
            entity.KieuGiam = request.KieuGiam;
            entity.NgayBatDau = request.NgayBatDau;
            entity.NgayKetThuc = request.NgayKetThuc;
            entity.SoLuong = request.SoLuong;
            entity.TrangThai = request.TrangThai;

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, message = "Cập nhật thành công!" });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PhieuGiamGia.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.PhieuGiamGia.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công!" });
        }

        [HttpPost("Claim")]
        public async Task<IActionResult>SuDungPhieu(SuDungPhieuResquest resquest)
        {
            var voucher = await _context.PhieuGiamGia
           .FirstOrDefaultAsync(x => x.Id == resquest.PhieuGiamGiaID);


            if (voucher == null)
                return BadRequest(new {
                 sucsess = false,
                 message ="Phiếu giảm giá không tồn tại"
                });

            if (DateTime.Now < voucher.NgayBatDau || DateTime.Now > voucher.NgayKetThuc)
                return BadRequest (new { 
                sussess = false,
                message ="phiếu này đã hết hạn sử dụng"
                });

            if (voucher.SoLuong <= 0)
                return BadRequest(new { sussess = false , message ="phiếu đã hết số lượng" });

            var userVoucher = await _context.TaiKhoanPhieuGiamGia
                                            .FirstOrDefaultAsync(x => x.TaiKhoanId == resquest.TaiKhoanID  
                                            && x.PhieuGiamGiaId == resquest.PhieuGiamGiaID);

            if(userVoucher != null)
            {
                return BadRequest(new {
                 success = false,
                 message ="Bạn đã nhận phiếu này rồi"
                });
            }

            var add = new TaiKhoanPhieuGiamGium
            {
               
                TaiKhoanId = resquest.TaiKhoanID,
                PhieuGiamGiaId = resquest .PhieuGiamGiaID,
                NgayNhan = DateTime.Now,
                DaSuDung = false,
            };

            _context.TaiKhoanPhieuGiamGia.Add(add);

            voucher.SoLuong -= 1;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "bạn nhận phiếu giảm giá thành công"
            });

        }
    }
}
