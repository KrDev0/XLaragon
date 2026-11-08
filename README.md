# XLaragon

**XLaragon es una aplicación complementaria para Laragon en Windows. No reemplaza a Laragon.** Se utiliza junto a una instalación existente y amplía algunas tareas de administración desde una interfaz propia. No incluye ni descarga Apache, Nginx, PHP, MySQL u otros servicios: trabaja con los componentes que ya tengas instalados en tu carpeta de Laragon.

Este es un proyecto comunitario no oficial y no está afiliado ni respaldado por el equipo de Laragon.

## Funciones

- Iniciar y detener servicios disponibles en la instalación de Laragon.
- Elegir versiones instaladas de algunos componentes.
- Editar preferencias y puertos de servicios.
- Crear sitios a partir de plantillas locales.
- Usar la interfaz en inglés o español, además de los paquetes de idioma encontrados en la instalación.

Las opciones **Auto Backup** y **Auto Update** guardan su preferencia, pero actualmente no ejecutan copias de seguridad ni actualizaciones.

## Requisitos

- Windows de 64 bits.
- Una instalación existente de Laragon con los componentes que quieras administrar.
- Para compilar desde el código: SDK de .NET 9 y Visual Studio con el componente de Windows Forms, o la CLI de .NET.

XLaragon busca una instalación junto al ejecutable y, si no la encuentra, comprueba `C:\laragon`. Asegúrate de ejecutar XLaragon desde la carpeta de la instalación que quieres administrar o de tener Laragon en esa ruta.

## Descargar

Consulta la sección **Releases** para descargar la versión publicada. Extrae el archivo ZIP y ejecuta `xlaragon.exe`. Mantén el ejecutable junto a tu instalación de Laragon, o usa `C:\laragon`.

> La descarga de una versión solo se anunciará aquí cuando haya un paquete de release disponible.

## Compilar

Desde esta carpeta (`app`):

```powershell
dotnet build Laragon.csproj -c Release
```

El ejecutable compilado se genera bajo `bin/Release/net9.0-windows/`.

## Alcance y datos

XLaragon modifica preferencias y archivos de configuración de la instalación de Laragon que administra. Haz una copia de seguridad de esos archivos antes de cambiar rutas, puertos o servicios.

## Nombre y marcas

Laragon es una marca de sus respectivos titulares. XLaragon es un proyecto independiente; los nombres se usan para describir la compatibilidad prevista.
