using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace SistemaVentaBlazor.Client.Servicios.Implementacion
{
    public class ProductoService : IProductoService
    {
        private readonly HttpClient _http;
        public ProductoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResponseDTO<ProductoDTO>> Crear(ProductoDTO entidad)
        {
            var result = await _http.PostAsJsonAsync("api/producto/Guardar", entidad);
            var response = await result.Content.ReadFromJsonAsync<ResponseDTO<ProductoDTO>>();
            return response!;
        }

        public async Task<bool> Editar(ProductoDTO entidad)
        {
            var result = await _http.PutAsJsonAsync("api/producto/Editar", entidad);
            var response = await result.Content.ReadFromJsonAsync<ResponseDTO<bool>>();

            return response!.status;
        }

        public async Task<bool> Eliminar(int id)
        {
            var result = await _http.DeleteAsync($"api/producto/Eliminar/{id}");
            var response = await result.Content.ReadFromJsonAsync<ResponseDTO<string>>();
            return response!.status;
        }

        public async Task<ResponseDTO<List<ProductoDTO>>> Lista()
        {
            var result = await _http.GetFromJsonAsync<ResponseDTO<List<ProductoDTO>>>("api/producto/Lista");
            return result!;
        }

        // Nuevo método para subir productos desde un archivo Excel
        public async Task<ResponseDTO<bool>> SubirProductosDesdeExcel(IBrowserFile archivo)
        {
            try
            {
                var content = new MultipartFormDataContent();

                // Convertir IBrowserFile a StreamContent para enviarlo
                using var stream = archivo.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                content.Add(fileContent, "archivo", archivo.Name);

                var result = await _http.PostAsync("api/producto/SubirExcel", content);
                var response = await result.Content.ReadFromJsonAsync<ResponseDTO<bool>>();

                return response ?? new ResponseDTO<bool> { status = false, msg = "Error al procesar el archivo" };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool> { status = false, msg = $"Error: {ex.Message}" };
            }
        }

        public async Task<ResponseDTO<bool>> ActualizarStock(int idProducto, int cantidad)
        {
            var request = new { IdProducto = idProducto, Cantidad = cantidad };
            var result = await _http.PutAsJsonAsync("api/producto/ActualizarStock", request);
            var response = await result.Content.ReadFromJsonAsync<ResponseDTO<bool>>();

            return response ?? new ResponseDTO<bool> { status = false, msg = "Error al actualizar stock" };
        }


    }
}
