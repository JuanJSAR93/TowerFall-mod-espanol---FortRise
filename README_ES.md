[Read in English](README.md)

# TowerFall Localization

Mod de localización y traducción para **TowerFall** utilizando **FortRise** y **Harmony**. Traduce el 100% de la interfaz, modos de juego, variantes, estadísticas, premios, arqueros y consejos.

## Características

- **Traducción integral:** Menús, opciones, controles, pausas, modos Versus, Quest, Dark World y Trials.
- **Variantes multi-idioma (`langs/`):** Incluye 536 cadenas completas para Español (`translations_ES.json`), Francés (`translations_FR.json`), Portugués de Brasil (`translations_PT_BR.json`), Alemán (`translations_DE.json`), Italiano (`translations_IT.json`) y Ruso (`translations_RU.json`).
- **Ajuste dinámico:** Formato y corte de línea equilibrado en premios y botones para evitar desbordes visuales.
- **Carga no invasiva:** Funciona en memoria mediante Harmony sin modificar los archivos XML ni los binarios originales del juego.

---

## Requisitos

- TowerFall instalado con [FortRise](https://github.com/FortRise/FortRise).
- .NET SDK `10.0.401` *(solo necesario para compilar el código fuente)*.
- La instalación debe contener `TowerFall.Patch.dll`, `0Harmony.dll` y `Microsoft.Extensions.Logging.Abstractions.dll`.

---

## Compilar e instalar

Haz doble clic en `compilar.bat`. El script compila el código y copia los archivos a tu instalación:

```text
Mods/TowerFallLocalization/TowerFallLocalization.dll
Mods/TowerFallLocalization/translations.json
```

La ruta predeterminada es:
```text
C:\Program Files (x86)\Steam\steamapps\common\TowerFall
```

También puedes especificar otra ruta de instalación como argumento:
```bat
compilar.bat "D:\Juegos\TowerFall - FortRise"
```

---

## Estructura del proyecto

```text
TowerFallLocalization/
├─ src/
│  └─ TowerFallEspanolModule.cs    # Módulo FortRise y parches Harmony
├─ langs/                          # Paquetes de idiomas adicionales
│  ├─ translations_ES.json
│  ├─ translations_FR.json
│  ├─ translations_PT_BR.json
│  ├─ translations_DE.json
│  ├─ translations_IT.json
│  └─ translations_RU.json
├─ translations.json               # Archivo activo de traducción (Español por defecto)
├─ meta.json                       # Manifiesto de mod para FortRise
├─ compilar.bat                    # Script de compilación e instalación
├─ README.md                       # Documentación en inglés
└─ README_ES.md                    # Documentación en español
```

Para usar otro idioma, simplemente copia el archivo deseado desde `langs/` y renómbralo como `translations.json`.

---

## Mecanismo de traducción

El módulo se carga como una DLL de FortRise. Al crearse, inicializa `TranslationService`, lee `translations.json` mediante `IModContent` y registra los parches de Harmony:

- **Dibujo de texto:** Intercepta `Monocle.Draw`, `Text` y `OutlineText`.
- **Botones y menús:** Traduce antes de medir los textos de `VariantButton` y `MenuButtonGuide` para que los globos y etiquetas se ajusten al ancho del texto traducido.
- **Arqueros:** Traduce los nombres y títulos en `ArcherData`.
- **Premios:** Intercepta `AwardInfo` y recalcula la división en dos líneas para evitar cortes inadecuados.
- **Fragmentos dinámicos:** Traduce textos divididos por separadores `|` y consejos en pantalla.

---

## Créditos y Agradecimientos

- **[FortRise](https://github.com/FortRise/FortRise):** El framework y cargador de mods oficial de la comunidad para TowerFall.
