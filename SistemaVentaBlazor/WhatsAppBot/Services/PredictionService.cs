using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using SistemaVentaBlazor.WhatsAppBot.Models.Prediction;
using System.Text.Json;

namespace SistemaVentaBlazor.WhatsAppBot.Services
{
    public class PredictionService
    {
        private readonly MLContext _mlContext;
        private readonly DataService _dataService;
        private readonly ILogger<PredictionService> _logger;

        public PredictionService(DataService dataService, ILogger<PredictionService> logger)
        {
            _mlContext = new MLContext(seed: 0);
            _dataService = dataService;
            _logger = logger;
        }

        public async Task<List<ProductoPronostico>> PronosticarInventarioAsync(int diasFuturos = 14)
        {
            try
            {
                // Obtener datos históricos y estado actual del inventario
                var ventasHistoricas = await _dataService.GetVentasHistoricasAsync(90);
                var productosActuales = await _dataService.GetProductosConInventarioAsync();

                var pronosticos = new List<ProductoPronostico>();

                // Agrupar ventas por producto
                var productoGroups = ventasHistoricas.GroupBy(v => v.ProductoId);

                foreach (var grupo in productoGroups)
                {
                    int productoId = grupo.Key;
                    var ventasProducto = grupo.OrderBy(v => v.Fecha).ToList();

                    // Solo pronosticar si hay suficientes datos (al menos 10 registros)
                    if (ventasProducto.Count < 10)
                    {
                        continue;
                    }

                    // Preparar datos para ML.NET
                    var dataView = _mlContext.Data.LoadFromEnumerable(ventasProducto);

                    // Configurar el algoritmo ARIMA para series temporales
                    var forecastingPipeline = _mlContext.Forecasting.ForecastBySsa(
                        outputColumnName: "ForecastedQuantity",
                        inputColumnName: nameof(VentaData.Cantidad),
                        windowSize: 7,
                        seriesLength: ventasProducto.Count,
                        trainSize: ventasProducto.Count,
                        horizon: diasFuturos,
                        confidenceLevel: 0.95f,
                        confidenceLowerBoundColumn: "LowerBound",
                        confidenceUpperBoundColumn: "UpperBound");

                    // Entrenar el modelo
                    var forecaster = forecastingPipeline.Fit(dataView);

                    // Realizar la predicción
                    var prediction = forecaster.Predict();

                    // Calcular la cantidad total pronosticada para el período
                    float cantidadPronosticada = prediction.ForecastedQuantity.Sum();

                    // Buscar el producto actual en el inventario
                    var productoActual = productosActuales.FirstOrDefault(p => (int)p.Id == productoId);
                    if (productoActual != null)
                    {
                        pronosticos.Add(new ProductoPronostico
                        {
                            ProductoId = productoId,
                            NombreProducto = productoActual.Nombre,
                            CantidadActual = productoActual.Stock,
                            CantidadPronosticada = cantidadPronosticada,
                            FechaPronostico = DateTime.Now.AddDays(diasFuturos)
                        });
                    }
                }

                return pronosticos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar pronóstico de inventario");
                return new List<ProductoPronostico>();
            }
        }

        public async Task<string> ObtenerRecomendacionesReabastecimientoAsync()
        {
            try
            {
                var pronosticos = await PronosticarInventarioAsync();

                var recomendaciones = pronosticos
                    .Where(p => p.RequiereReabastecimiento)
                    .OrderByDescending(p => p.CantidadPronosticada / p.CantidadActual)
                    .Select(p => new
                    {
                        ProductoId = p.ProductoId,
                        Nombre = p.NombreProducto,
                        StockActual = p.CantidadActual,
                        DemandaPronosticada = p.CantidadPronosticada,
                        CantidadSugerida = Math.Ceiling(p.CantidadPronosticada * 1.2) - p.CantidadActual,
                        PorcentajeAgotamiento = (p.CantidadPronosticada / p.CantidadActual) * 100
                    })
                    .ToList();

                return JsonSerializer.Serialize(recomendaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recomendaciones de reabastecimiento");
                return "[]";
            }
        }

        public async Task<string> ObtenerAnalisisTendenciasAsync()
        {
            try
            {
                var ventasHistoricas = await _dataService.GetVentasHistoricasAsync(90);

                // Agrupar por fecha (día) y sumar cantidades
                var ventasPorDia = ventasHistoricas
                    .GroupBy(v => v.Fecha.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key.ToString("yyyy-MM-dd"),
                        TotalVendido = g.Sum(v => v.Cantidad)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToList();

                // Detectar tendencias
                var tendencia = DetectarTendencia(ventasPorDia.Select(v => v.TotalVendido).ToList());

                var resultado = new
                {
                    DatosVentas = ventasPorDia,
                    Tendencia = tendencia,
                    DiasAnalisis = ventasPorDia.Count,
                    PromedioVentasDiarias = ventasPorDia.Average(v => v.TotalVendido),
                    FechaAnalisis = DateTime.Now.ToString("yyyy-MM-dd")
                };

                return JsonSerializer.Serialize(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener análisis de tendencias");
                return "{}";
            }
        }

        private string DetectarTendencia(List<float> datos)
        {
            if (datos.Count < 2)
                return "Sin datos suficientes";

            // Cálculo simple de tendencia lineal
            float primerTercio = datos.Take(datos.Count / 3).Average();
            float ultimoTercio = datos.Skip(2 * datos.Count / 3).Average();

            float diferenciaPorcentual = ((ultimoTercio - primerTercio) / primerTercio) * 100;

            if (diferenciaPorcentual > 10)
                return "Fuerte tendencia al alza";
            else if (diferenciaPorcentual > 5)
                return "Tendencia moderada al alza";
            else if (diferenciaPorcentual < -10)
                return "Fuerte tendencia a la baja";
            else if (diferenciaPorcentual < -5)
                return "Tendencia moderada a la baja";
            else
                return "Estable";
        }
    }
}