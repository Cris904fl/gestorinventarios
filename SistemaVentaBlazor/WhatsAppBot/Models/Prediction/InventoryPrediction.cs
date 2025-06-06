using Microsoft.ML.Data;

namespace SistemaVentaBlazor.WhatsAppBot.Models.Prediction
{
    public class VentaData
    {
        [LoadColumn(0)]
        public int ProductoId { get; set; }

        [LoadColumn(1)]
        public float Cantidad { get; set; }

        [LoadColumn(2)]
        public DateTime Fecha { get; set; }
    }

    public class InventoryPrediction
    {
        public float PredictedQuantity { get; set; }
        public float Score { get; set; }
    }

    public class ProductoPronostico
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public float CantidadActual { get; set; }
        public float CantidadPronosticada { get; set; }
        public DateTime FechaPronostico { get; set; }
        public bool RequiereReabastecimiento => CantidadPronosticada > CantidadActual * 0.7;
    }
}