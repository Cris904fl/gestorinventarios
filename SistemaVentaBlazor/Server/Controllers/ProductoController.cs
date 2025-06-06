using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentaBlazor.Server.Models;
using SistemaVentaBlazor.Server.Repositorio.Contrato;
using SistemaVentaBlazor.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;


namespace SistemaVentaBlazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IProductoRepositorio _productoRepositorio;

        public ProductoController(IProductoRepositorio productoRepositorio, IMapper mapper)
        {
            _mapper = mapper;
            _productoRepositorio = productoRepositorio;
        }

        // Obtener lista de productos
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            ResponseDTO<List<ProductoDTO>> _ResponseDTO = new ResponseDTO<List<ProductoDTO>>();

            try
            {
                List<ProductoDTO> ListaProductos = new List<ProductoDTO>();
                IQueryable<Producto> query = await _productoRepositorio.Consultar();
                query = query.Include(r => r.IdCategoriaNavigation);

                ListaProductos = _mapper.Map<List<ProductoDTO>>(query.ToList());

                if (ListaProductos.Count > 0)
                    _ResponseDTO = new ResponseDTO<List<ProductoDTO>>() { status = true, msg = "ok", value = ListaProductos };
                else
                    _ResponseDTO = new ResponseDTO<List<ProductoDTO>>() { status = false, msg = "", value = null };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<List<ProductoDTO>>() { status = false, msg = ex.Message, value = null };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        // Guardar nuevo producto
        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] ProductoDTO request)
        {
            ResponseDTO<ProductoDTO> _ResponseDTO = new ResponseDTO<ProductoDTO>();

            try
            {
                Producto _producto = _mapper.Map<Producto>(request);
                Producto _productoCreado = await _productoRepositorio.Crear(_producto);

                if (_productoCreado.IdProducto != 0)
                    _ResponseDTO = new ResponseDTO<ProductoDTO>() { status = true, msg = "ok", value = _mapper.Map<ProductoDTO>(_productoCreado) };
                else
                    _ResponseDTO = new ResponseDTO<ProductoDTO>() { status = false, msg = "No se pudo crear el producto" };

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<ProductoDTO>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        // Editar producto
        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] ProductoDTO request)
        {
            ResponseDTO<bool> _ResponseDTO = new ResponseDTO<bool>();

            try
            {
                Producto _producto = _mapper.Map<Producto>(request);
                Producto _productoParaEditar = await _productoRepositorio.Obtener(u => u.IdProducto == _producto.IdProducto);

                if (_productoParaEditar != null)
                {
                    _productoParaEditar.Nombre = _producto.Nombre;
                    _productoParaEditar.IdCategoria = _producto.IdCategoria;
                    _productoParaEditar.Stock = _producto.Stock;
                    _productoParaEditar.Precio = _producto.Precio;

                    bool respuesta = await _productoRepositorio.Editar(_productoParaEditar);

                    if (respuesta)
                        _ResponseDTO = new ResponseDTO<bool>() { status = true, msg = "ok", value = true };
                    else
                        _ResponseDTO = new ResponseDTO<bool>() { status = false, msg = "No se pudo editar el producto" };
                }
                else
                {
                    _ResponseDTO = new ResponseDTO<bool>() { status = false, msg = "No se encontró el producto" };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<bool>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        // Eliminar producto
        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            ResponseDTO<string> _ResponseDTO = new ResponseDTO<string>();

            try
            {
                Producto _productoEliminar = await _productoRepositorio.Obtener(u => u.IdProducto == id);

                if (_productoEliminar != null)
                {
                    bool respuesta = await _productoRepositorio.Eliminar(_productoEliminar);

                    if (respuesta)
                        _ResponseDTO = new ResponseDTO<string>() { status = true, msg = "ok", value = "" };
                    else
                        _ResponseDTO = new ResponseDTO<string>() { status = false, msg = "No se pudo eliminar el producto", value = "" };
                }

                return StatusCode(StatusCodes.Status200OK, _ResponseDTO);
            }
            catch (Exception ex)
            {
                _ResponseDTO = new ResponseDTO<string>() { status = false, msg = ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _ResponseDTO);
            }
        }

        // Subir productos desde un archivo Excel
        [HttpPost]
        [Route("SubirExcel")]
        public async Task<IActionResult> SubirExcel([FromForm] IFormFile archivo)
        {
            ResponseDTO<bool> response = new ResponseDTO<bool>();

            if (archivo == null || archivo.Length == 0)
            {
                response.status = false;
                response.msg = "Archivo vacío o nulo.";
                return BadRequest(response);
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Necesario para EPPlus

                using var stream = archivo.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var productos = new List<Producto>();

                var worksheet = package.Workbook.Worksheets[0]; // Primera hoja
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Saltar la primera fila (encabezados)
                {
                    var producto = new Producto
                    {
                        Nombre = worksheet.Cells[row, 1].Text,
                        IdCategoria = int.Parse(worksheet.Cells[row, 2].Text),
                        Stock = int.Parse(worksheet.Cells[row, 3].Text),
                        Precio = decimal.Parse(worksheet.Cells[row, 4].Text),
                        EsActivo = true,
                        FechaRegistro = DateTime.Now
                    };
                    productos.Add(producto);
                }

                bool resultado = await _productoRepositorio.AgregarProductosMasivo(productos);

                if (resultado)
                {
                    response.status = true;
                    response.msg = "Productos subidos correctamente";
                    return Ok(response);
                }
                else
                {
                    response.status = false;
                    response.msg = "Error al subir productos";
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.msg = $"Error: {ex.Message}";
                return StatusCode(500, response);
            }
        }

        [HttpPut("ActualizarStock")]
        public async Task<IActionResult> ActualizarStock([FromBody] StockUpdate request)
        {
            bool resultado = await _productoRepositorio.ActualizarStock(request.IdProducto, request.Cantidad);

            if (resultado)
                return Ok(new ResponseDTO<bool> { status = true, msg = "Stock actualizado correctamente" });
            else
                return BadRequest(new ResponseDTO<bool> { status = false, msg = "No se pudo actualizar el stock" });
        }

    }
}
