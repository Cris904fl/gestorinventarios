using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Json;
using SistemaVentaBlazor.WhatsAppBot.Models.Prediction;

namespace SistemaVentaBlazor.WhatsAppBot.Services
{
    public class DataService
    {
        private readonly string _connectionString;
        private readonly ILogger<DataService> _logger;

        public DataService(IConfiguration configuration, ILogger<DataService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        public async Task<List<VentaData>> GetVentasHistoricasAsync(int diasAtras = 90)
        {
            var result = new List<VentaData>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    SELECT 
                        dv.ProductoId,
                        dv.Cantidad,
                        v.Fecha
                    FROM 
                        DetalleVenta dv
                    INNER JOIN 
                        Venta v ON dv.VentaId = v.Id
                    WHERE 
                        v.Fecha >= @fechaInicio
                    ORDER BY 
                        v.Fecha", connection);

                command.Parameters.AddWithValue("@fechaInicio", DateTime.Now.AddDays(-diasAtras));

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new VentaData
                    {
                        ProductoId = reader.GetInt32(0),
                        Cantidad = reader.GetFloat(1),
                        Fecha = reader.GetDateTime(2)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas históricas");
            }

            return result;
        }

        public async Task<List<dynamic>> GetProductosConInventarioAsync()
        {
            var result = new List<dynamic>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    SELECT 
                        p.Id,
                        p.Nombre,
                        p.Precio,
                        p.Stock,
                        c.Nombre AS Categoria
                    FROM 
                        Producto p
                    INNER JOIN 
                        Categoria c ON p.CategoriaId = c.Id
                    ORDER BY 
                        p.Nombre", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        Stock = reader.GetInt32(3),
                        Categoria = reader.GetString(4)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con inventario");
            }

            return result;
        }

        public async Task<string> GetResumenVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    SELECT 
                        COUNT(DISTINCT v.Id) AS TotalVentas,
                        SUM(v.Total) AS MontoTotal,
                        COUNT(DISTINCT v.UsuarioId) AS ClientesUnicos
                    FROM 
                        Venta v
                    WHERE 
                        v.Fecha BETWEEN @fechaInicio AND @fechaFin", connection);

                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var resumen = new
                    {
                        TotalVentas = reader.GetInt32(0),
                        MontoTotal = reader.GetDecimal(1),
                        ClientesUnicos = reader.GetInt32(2),
                        FechaInicio = fechaInicio.ToShortDateString(),
                        FechaFin = fechaFin.ToShortDateString()
                    };

                    return JsonSerializer.Serialize(resumen);
                }

                return "{}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de ventas");
                return "{}";
            }
        }

        public async Task<string> GetProductosMasVendidosAsync(int top = 5)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    SELECT TOP (@top)
                        p.Id,
                        p.Nombre,
                        SUM(dv.Cantidad) AS TotalVendido,
                        SUM(dv.Cantidad * dv.PrecioUnitario) AS MontoTotal
                    FROM 
                        DetalleVenta dv
                    INNER JOIN 
                        Producto p ON dv.ProductoId = p.Id
                    INNER JOIN 
                        Venta v ON dv.VentaId = v.Id
                    WHERE 
                        v.Fecha >= DATEADD(day, -30, GETDATE())
                    GROUP BY 
                        p.Id, p.Nombre
                    ORDER BY 
                        TotalVendido DESC", connection);

                command.Parameters.AddWithValue("@top", top);

                var productos = new List<dynamic>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    productos.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        TotalVendido = reader.GetDouble(2),
                        MontoTotal = reader.GetDecimal(3)
                    });
                }

                return JsonSerializer.Serialize(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return "[]";
            }
        }

        public async Task<string> GetProductosBajoStockAsync(int umbral = 10)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    SELECT 
                        p.Id,
                        p.Nombre,
                        p.Stock,
                        c.Nombre AS Categoria
                    FROM 
                        Producto p
                    INNER JOIN 
                        Categoria c ON p.CategoriaId = c.Id
                    WHERE 
                        p.Stock <= @umbral
                    ORDER BY 
                        p.Stock", connection);

                command.Parameters.AddWithValue("@umbral", umbral);

                var productos = new List<dynamic>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    productos.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Stock = reader.GetInt32(2),
                        Categoria = reader.GetString(3)
                    });
                }

                return JsonSerializer.Serialize(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                return "[]";
            }
        }
    }
}