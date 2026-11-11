# Sistema de Gestión de Visitas Técnicas - Tecnoware Solutions 🚀

Plataforma integral desarrollada para la digitalización, control y ejecución de visitas técnicas en sitio. Este sistema permite agendar servicios, registrar el estado del inventario tecnológico de los clientes, recopilar firmas digitales y generar actas automáticas en PDF.

## 🛠️ Tecnologías Utilizadas

* **Frontend:** Blazor WebAssembly (WASM), HTML5, CSS3, Bootstrap 5.
* **Backend:** ASP.NET Core Web API, C# (.NET).
* **Base de Datos:** SQL Server, Entity Framework Core.
* **Seguridad:** Autenticación y Autorización basada en JSON Web Tokens (JWT).
* **Generación de Documentos:** iTextSharp / QuestPDF (Generación de actas PDF).
* **Notificaciones:** SMTP (Envío automático de actas por correo).

## ✨ Características Principales

* **Control de Roles:** Acceso segmentado para Administradores y Técnicos (Trabajadores).
* **Gestión de Clientes e Inventario:** Registro de empresas y sus dispositivos vinculados.
* **Plantillas Dinámicas:** Creación de formularios de evaluación y checklists personalizables.
* **Asignación de Visitas:** Panel de administrador para agendar y asignar visitas a técnicos específicos.
* **Ejecución en Terreno:** 
  * Selección y evaluación de equipos en sitio.
  * Carga de evidencias fotográficas.
  * Captura de firma digital del cliente en pantalla.
* **Automatización:** Cierre de visita con generación automática de un acta PDF y envío instantáneo al correo del cliente.

## ⚙️ Requisitos Previos

* [.NET SDK](https://dotnet.microsoft.com/download) (Versión correspondiente a la solución, ej. .NET 8 o superior).
* SQL Server (LocalDB o instancia de servidor).
* Herramientas de EF Core (`dotnet tool install --global dotnet-ef`).

## 🚀 Instalación y Configuración

1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/tu-usuario/SistemaVisitas.git](https://github.com/tu-usuario/SistemaVisitas.git)
   cd SistemaVisitas
