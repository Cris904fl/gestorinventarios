using Microsoft.EntityFrameworkCore;
using SistemaVentaBlazor.Server.Models;
using SistemaVentaBlazor.Server.Repositorio.Contrato;
using System.Linq.Expressions;

namespace SistemaVentaBlazor.Server.Repositorio.Implementacion
{
    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly DbventaBlazorContext _dbContext;

        public ProductoRepositorio(DbventaBlazorContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IQueryable<Producto>> Consultar(Expression<Func<Producto, bool>> filtro = null)
        {
            IQueryable<Producto> queryEntidad = filtro == null ? _dbContext.Productos : _dbContext.Productos.Where(filtro);
            return queryEntidad;
        }

        public async Task<Producto> Crear(Producto entidad)
        {
            try
            {
                _dbContext.Set<Producto>().Add(entidad);
                await _dbContext.SaveChangesAsync();
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(Producto entidad)
        {
            try
            {
                _dbContext.Update(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(Producto entidad)
        {
            try
            {
                _dbContext.Remove(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Producto> Obtener(Expression<Func<Producto, bool>> filtro = null)
        {
            try
            {
                return await _dbContext.Productos.Where(filtro).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> AgregarProductosMasivo(List<Producto> productos)
        {
            try
            {
                await _dbContext.Productos.AddRangeAsync(productos);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ActualizarStock(int idProducto, int cantidad)
        {
            try
            {
                var producto = await _dbContext.Productos.FindAsync(idProducto);
                if (producto == null)
                {
                    return false; // No se encontró el producto
                }

                producto.Stock += cantidad; // Actualizar stock
                _dbContext.Update(producto);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }


    }
}
