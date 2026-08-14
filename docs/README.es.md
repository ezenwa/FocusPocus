<p align="center">
  <img src="assets/focuspocus-banner-v2.png" alt="FocusPocus — Magia que sigue tus movimientos" width="100%">
</p>

# FocusPocus

FocusPocus es una utilidad moderna para Windows 11 que destaca el puntero, visualiza los clics y muestra teclas o atajos durante presentaciones, grabaciones y demostraciones.

[Descargar la versión más reciente](https://github.com/ezenwa/FocusPocus/releases/latest) · [README en inglés](../README.md)

## Funciones

- Foco ajustable hasta 800 px con difuminado de contorno.
- Transiciones suaves entre monitores.
- Color y opacidad configurables para el overlay.
- Visualización de clics y sonido opcional.
- Visualización de teclas con modo «solo atajos» y protección de campos de contraseña.
- Atajos globales para foco, clics, teclas, tamaño y opacidad.
- Interfaz nativa WinUI 3 con Mica y controles de Windows 11.
- Inicio con Windows directamente en la bandeja.
- Búsqueda de actualizaciones mediante GitHub Releases.
- Interfaz en español e inglés.

## Instalación

1. Abre la [última publicación](https://github.com/ezenwa/FocusPocus/releases/latest).
2. Descarga `FocusPocus-Setup-<versión>.exe`.
3. Ejecuta el instalador.

## Privacidad

Los eventos del puntero y teclado se procesan localmente. FocusPocus no transmite las entradas capturadas y oculta las teclas cuando un campo de contraseña estándar de Windows tiene el foco. La búsqueda de actualizaciones solo consulta metadatos públicos de GitHub cuando el usuario pulsa el botón correspondiente.

## Compilación

Requiere .NET 8 SDK. Inno Setup 6 solo es necesario para generar el instalador.

```powershell
dotnet build .\src\FocusPocus.UI\FocusPocus.UI.csproj -c Release
dotnet build .\src\SpotDot\SpotDot.csproj -c Release
.\build.ps1
```

## Licencia

[MIT](../LICENSE)
