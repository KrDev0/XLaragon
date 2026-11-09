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

En [Releases](https://github.com/KrDev0/XLaragon/releases/latest) hay dos paquetes para Windows x64:

- [**XLaragon Lite**](https://github.com/KrDev0/XLaragon/releases/latest/download/XLaragon-Lite-v1.1.0-win-x64.zip): contiene el ensamblado y necesita tener instalado el **.NET 9 Desktop Runtime** (x64).
- [**XLaragon autocontenido**](https://github.com/KrDev0/XLaragon/releases/latest/download/XLaragon-Standalone-v1.1.0-win-x64.zip): incluye el runtime y no necesita instalar .NET por separado; ocupa más espacio.

Descarga y extrae una de las opciones. Ejecuta `xlaragon.exe` desde la carpeta raíz de tu instalación de Laragon, o desde una carpeta con una instalación en `C:\laragon`.

## Compilar

Desde esta carpeta (`app`):

```powershell
dotnet build Laragon.csproj -c Release
```

El ejecutable compilado se genera bajo `bin/Release/net9.0-windows/`.

## Alcance y datos

XLaragon modifica preferencias y archivos de configuración de la instalación de Laragon que administra. Haz una copia de seguridad de esos archivos antes de cambiar rutas, puertos o servicios.

Los ajustes se guardan en `usr\laragon.ini` **de la instalación administrada** (por ejemplo `C:\laragon\usr\laragon.ini`), no en la carpeta del programa, y al guardar solo se reescriben las claves que cambian: el resto del archivo se conserva tal cual.

Al cambiar la versión de PHP también se actualizan los archivos de configuración del servidor web que llevan la ruta de PHP (`etc\apache2\mod_php.conf`, `etc\apache2\fcgid.conf` y los `*.conf` de `etc\nginx`), para que el servidor y el `laragon.ini` nombren siempre la misma carpeta.

Al abrirse normalmente, XLaragon pide permisos de administrador y Windows muestra su aviso; si se cancela, el programa sigue sin ellos y las operaciones que requieren esos permisos pueden fallar. Los modificadores de diagnóstico para capturas, listados y pruebas no piden elevación ni modifican archivos. `--phpsync` es una excepción: repara las rutas PHP de las configuraciones de Apache y Nginx y escribe los archivos que corrige.

## Nombre y marcas

Laragon es una marca de sus respectivos titulares. XLaragon es un proyecto independiente; los nombres se usan para describir la compatibilidad prevista.
