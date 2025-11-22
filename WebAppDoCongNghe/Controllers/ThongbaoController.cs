using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppDoCongNghe.Models.model;
using WebAppDoCongNghe.Service;

namespace WebAppDoCongNghe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongbaoController : ControllerBase
    {
        private readonly PublicService _publicService;

        public ThongbaoController(PublicService publicService)
        {
            _publicService = publicService;
        }

        [HttpGet]
        public async Task<IActionResult> getThongBao(int idTaiKhoan)
        {
            var result = await _publicService.GetByUser(idTaiKhoan);
            return Ok(result);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var result = await _publicService.GetDetail(id);
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddThongBao([FromBody] ThongBaoModel model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ."
                });
            }

            var result = await _publicService.AddThongBao(model.TaiKhoanId, model.TieuDe, model.NoiDung);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteThongBao(int id)
        {
            var result = await _publicService.DeleteThongBao(id);
            return Ok(result);
        }
    }
}
