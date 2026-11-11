using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EvidenciasController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        public EvidenciasController(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        [HttpPost("SubirFoto")]
        public async Task<IActionResult> SubirFoto(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0) return BadRequest("No se recibió ningún archivo.");

            // 1. Abrimos el archivo que llegó del frontend
            using var stream = archivo.OpenReadStream();

            // 2. Configuramos la subida a Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(archivo.FileName, stream),
                Transformation = new Transformation().Height(800).Width(800).Crop("limit"), // Compresión inteligente
                Folder = "Tecnoware_Visitas" // Carpeta en tu nube
            };

            // 3. Ejecutamos la subida
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return StatusCode(500, uploadResult.Error.Message);
            }

            // 4. Devolvemos únicamente el enlace público
            return Ok(new { url = uploadResult.SecureUrl.ToString() });
        }
    }
}