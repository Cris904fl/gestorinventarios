# 📦 SnapPOS - Gestor de Inventario Inteligente

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Hybrid-512BD4?style=flat&logo=blazor)](https://blazor.net/)
[![Python](https://img.shields.io/badge/Python-3.8+-3776AB?style=flat&logo=python)](https://python.org/)
[![AI](https://img.shields.io/badge/AI-Machine%20Learning-FF6F00?style=flat&logo=tensorflow)](https://tensorflow.org/)

**SnapPOS** es una aplicación híbrida de gestión de inventario que integra **Inteligencia Artificial** para automatizar procesos, predecir demanda y optimizar el control de stock.

## 🚀 Características Principales

- **📱 Multi-plataforma**: Windows, macOS, Linux, iOS, Android
- **🤖 IA Integrada**: Predicción de ventas y análisis de demanda
- **🧾 OCR Inteligente**: Carga automática de productos desde facturas
- **📊 Reportes Avanzados**: Dashboards con métricas en tiempo real
- **🔄 Sincronización**: Trabajo offline/online automático

## 🛠️ Stack Tecnológico

**Frontend**: .NET MAUI + Blazor Hybrid + C#  
**Backend**: Entity Framework Core + SQLite/SQL Server  
**IA**: Python + Flask + Scikit-learn + TensorFlow + OCR

## 📋 Instalación Rápida

### 1. Clonar Repositorio
```bash
git clone https://github.com/Cris904fl/gestorinventarios.git
cd gestorinvetarios
```

### 2. Configurar .NET
```bash
dotnet restore
dotnet ef database update
dotnet build
```

### 3. Configurar IA (Python)
```bash
cd SnapPOS.AI
python -m venv venv
venv\Scripts\activate  # Windows
pip install -r requirements.txt
python app.py
```

### 4. Ejecutar Aplicación
```bash
dotnet run --project SnapPOS.MAUI
```

## 📁 Estructura del Proyecto

```
gestorinvetarios/
├── 📂 SnapPOS.MAUI/          # Aplicación principal
├── 📂 SnapPOS.Core/          # Lógica de negocio
├── 📂 SnapPOS.Data/          # Capa de datos
├── 📂 SnapPOS.AI/            # Motor de IA (Python)
└── 📂 SnapPOS.Tests/         # Pruebas
```

## 🔧 Configuración

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=snappos.db"
  },
  "AISettings": {
    "PythonApiUrl": "http://localhost:5000",
    "EnableOCR": true,
    "EnablePredictions": true
  }
}
```

## 🤖 Funcionalidades de IA

### Predicción de Ventas
```csharp
var prediccion = await _aiService.PredecirVentas(new PredictionRequest
{
    ProductoId = 1,
    DiasAPredecir = 30
});
```

### Procesamiento de Facturas
```csharp
var resultado = await _ocrService.ProcesarFactura(imagenFactura);
await _inventarioService.ActualizarStock(resultado.Productos);
```

## 📊 API Endpoints

```http
# Inventario
GET    /api/productos              # Listar productos
POST   /api/productos              # Crear producto
PUT    /api/productos/{id}         # Actualizar producto

# IA
POST   /api/ai/predict-sales       # Predicción de ventas
POST   /api/ai/process-invoice     # Procesar factura
GET    /api/ai/health              # Estado del sistema
```

## 📱 Despliegue

```bash
# Android
dotnet publish -f net8.0-android -c Release

# iOS
dotnet publish -f net8.0-ios -c Release

# Windows
dotnet publish -f net8.0-windows10.0.19041.0 -c Release
```

## 🧪 Testing

```bash
# Pruebas .NET
dotnet test

# Pruebas Python
cd SnapPOS.AI && python -m pytest
```

## 📈 Roadmap

- **Q2 2024**: Dashboard avanzado, APIs de proveedores
- **Q3 2024**: App móvil optimizada, códigos QR/Barcode
- **Q4 2024**: Marketplace de plugins, versión cloud

## 🆘 Soporte

- **Documentación**: [docs.snappos.com](https://docs.snappos.com)
- **Issues**: [GitHub Issues](https://github.com/Cris904fl/gestorinvetarios/issues)
- **Email**: cristian.florez904@gmail.com

## 📜 Licencia

MIT License - ver [LICENSE](LICENSE) para detalles.

---

**Desarrollado con ❤️ por Cristian Florez usando .NET MAUI Blazor Hybrid y Python AI** 🚀
