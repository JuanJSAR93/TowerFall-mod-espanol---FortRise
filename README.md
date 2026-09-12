# TowerFall Español

Mod de traducción al español para TowerFall usando FortRise y Harmony.

## Requisitos

- TowerFall instalado con FortRise.
- .NET SDK `10.0.401`.
- La instalación debe contener `TowerFall.Patch.dll`, `0Harmony.dll` y `Microsoft.Extensions.Logging.Abstractions.dll`.

El compilador utiliza las referencias del SDK y las DLL de la instalación de TowerFall. Por eso no se incluyen binarios del juego en este repositorio.

## Compilar e instalar

Haz doble clic en `compilar.bat`. El script compila el código y copia estos archivos al mod instalado:

```text
Mods/TowerFallEspanol/TowerFallEspanol.dll
Mods/TowerFallEspanol/translations.json
```

La ruta predeterminada es:

```text
C:\Program Files (x86)\Steam\steamapps\common\TowerFall - FortRise
```

También se puede pasar otra instalación como primer argumento:

```bat
compilar.bat "D:\Juegos\TowerFall - FortRise"
```

El script no ejecuta el juego. Si la copia falla, cierra TowerFall y vuelve a compilar.

## Estructura

```text
repo/
├─ src/
│  └─ TowerFallEspanolModule.cs
├─ translations.json
├─ meta.json
├─ compilar.bat
└─ README.md
```

`translations.json` es el archivo editable de traducciones. Las claves son los textos originales en inglés y los valores son sus equivalentes en español. Se conservan mayúsculas, signos y separadores `|` cuando forman parte del texto original.

## Mecanismo de traducción

El módulo se carga como una DLL de FortRise. Al crearse, inicializa `TranslationService`, lee `translations.json` mediante `IModContent` y registra los parches de Harmony.

La traducción funciona principalmente en estos puntos:

- Intercepta las funciones de dibujo de `Monocle.Draw`, `Text` y `OutlineText`, cubriendo textos dibujados directamente y textos almacenados en componentes.
- Traduce antes de medir los textos de `VariantButton` y `MenuButtonGuide`. Esto permite que los globos de variantes y las etiquetas de botones se ajusten al ancho español.
- Traduce nombres de arqueros cuando `ArcherData` termina de inicializarse.
- Traduce premios mediante `AwardInfo`. En el recuento final reconstruye el nombre completo, lo traduce y luego lo divide equilibradamente en líneas para evitar cortes.
- Traduce fragmentos dinámicos y textos divididos por `|`, como lecciones y consejos.

La traducción es conservadora: si una clave no existe, el texto original se mantiene. Las entradas se cargan desde el JSON del mod, por lo que no es necesario reemplazar los XML originales del juego.

## Fuente de datos

Los textos se obtuvieron de los ensamblados de TowerFall/FortRise y de los datos de `Content/Atlas/GameData`. Las lecciones, consejos y nombres de temas se integraron en `translations.json`.

## Nota sobre caracteres

El juego puede utilizar fuentes sin todos los caracteres Unicode. El módulo normaliza internamente los valores que se dibujan para evitar errores de `SpriteFont`; por eso conviene probar dentro del juego cualquier traducción con tildes, `ñ` o símbolos especiales.
