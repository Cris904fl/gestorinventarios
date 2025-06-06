using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace SistemaVentaBlazor.Client.Servicios.Contrato
{
    public interface IProductoService
    {
        Task<ResponseDTO<List<ProductoDTO>>> Lista();
        Task<ResponseDTO<ProductoDTO>> Crear(ProductoDTO entidad);
        Task<bool> Editar(ProductoDTO entidad);
        Task<bool> Eliminar(int id);

        // Nuevo método para subir productos desde un archivo Excel
        Task<ResponseDTO<bool>> SubirProductosDesdeExcel(IBrowserFile archivo);
        Task<ResponseDTO<bool>> ActualizarStock(int idProducto, int cantidad);

    }


}
