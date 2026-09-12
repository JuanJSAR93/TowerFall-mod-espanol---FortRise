using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;

namespace TowerFallEspanol;

public sealed class TowerFallEspanolModule : Mod
{
    public TowerFallEspanolModule(IModContent content, IModuleContext context, ILogger logger)
        : base(content, context, logger)
    {
        TranslationService.Initialize(content);
        context.Harmony.PatchAll(Assembly.GetExecutingAssembly());
        ArcherNamePatch.ApplyTranslations();
        logger.LogInformation("TowerFallEspanol: traducciones registradas.");
    }
}

internal static class TranslationService
{
    private static readonly Dictionary<string, string> Translations = CreateDefaults();
    private static readonly Dictionary<string, string> TranslationCache = new(StringComparer.Ordinal);

    internal static void Initialize(IModContent content)
    {
        TranslationCache.Clear();
        try
        {
            using var stream = content.OpenStream("translations.json");
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
            if (values == null) return;
            foreach (var pair in values)
                Translations[pair.Key] = NormalizeFontText(pair.Value);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"TowerFallEspanol: no se pudo leer translations.json: {exception.Message}");
        }
    }

    internal static void TranslateArguments(object[] args)
    {
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] is string text)
                args[index] = Translate(text);
        }
    }

    internal static string Translate(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (TranslationCache.TryGetValue(text, out var cached)) return cached;

        var exact = text.Trim();
        if (Translations.TryGetValue(exact, out var translated))
            return TranslationCache[text] = text.Replace(exact, translated, StringComparison.Ordinal);

        var parts = text.Split('|');
        var changed = false;
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index].Trim();
            if (!Translations.TryGetValue(part, out translated)) continue;
            parts[index] = parts[index].Replace(part, translated, StringComparison.Ordinal);
            changed = true;
        }

        return TranslationCache[text] = changed ? string.Join('|', parts) : TranslateKnownFragments(text);
    }

    // Algunos textos incluyen datos variables, por ejemplo "CONTROLLER P1 (...)"
    // o "CONTINUE (A)". Se traducen por fragmentos completos sin alterar rutas,
    // identificadores ni palabras que formen parte de otra palabra.
    private static string TranslateKnownFragments(string text)
    {
        var result = text;

        foreach (var pair in Translations.OrderByDescending(pair => pair.Key.Length))
        {
            if (pair.Key.Length < 3 || string.IsNullOrWhiteSpace(pair.Key)) continue;

            var searchIndex = 0;
            while (searchIndex < result.Length)
            {
                var index = result.IndexOf(pair.Key, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0) break;

                if (!HasWordBoundaries(result, index, pair.Key.Length))
                {
                    searchIndex = index + pair.Key.Length;
                    continue;
                }

                result = result.Remove(index, pair.Key.Length).Insert(index, pair.Value);
                searchIndex = index + pair.Value.Length;
            }
        }

        return result;
    }

    private static bool HasWordBoundaries(string text, int start, int length)
    {
        var end = start + length;
        return (start == 0 || !IsWordCharacter(text[start - 1])) &&
               (end == text.Length || !IsWordCharacter(text[end]));
    }

    private static bool IsWordCharacter(char character) =>
        char.IsLetterOrDigit(character) || character == '_';

    // La SpriteFont de TowerFall no contiene letras acentuadas ni eñes. Normalizar
    // el archivo editable evita que un cambio manual en el JSON cierre el juego.
    private static string NormalizeFontText(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var normalized = value.Normalize(NormalizationForm.FormD);
        var safe = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (character == '\r' || character == '\n' || character == '\t' ||
                (character >= ' ' && character <= '~'))
                safe.Append(character);
        }

        return safe.ToString();
    }

    private static Dictionary<string, string> CreateDefaults()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["OPTIONS"] = "OPCIONES",
            ["QUIT"] = "SALIR",
            ["CREDITS"] = "CREDITOS",
            ["TRIALS"] = "PRUEBAS",
            ["ARCHIVES"] = "ARCHIVOS"
        };

        return result;
    }
}

[HarmonyPatch]
internal static class DrawTextPatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var drawType = AccessTools.TypeByName("Monocle.Draw");
        if (drawType == null) yield break;

        foreach (var method in drawType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (!method.Name.StartsWith("Text", StringComparison.Ordinal) &&
                !method.Name.StartsWith("OutlineText", StringComparison.Ordinal))
                continue;

            if (method.GetParameters().Any(parameter => parameter.ParameterType == typeof(string)))
                yield return method;
        }
    }

    private static void Prefix(object[] __args) => TranslationService.TranslateArguments(__args);
}

[HarmonyPatch]
internal static class TextComponentPatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var typeNames = new[]
        {
            "TowerFall.OptionsButton",
            "TowerFall.Patching.OptionsButton",
            "FortRise.OptionsButtonHeader",
            "FortRise.GamepadInputOptionsButton",
            "FortRise.KeyboardInputOptionsButton",
            "FortRise.ModOptionsButton",
            "FortRise.UIMusicList+MusicOptionsButton",
            "TowerFall.MainModeButton",
            "TowerFall.MenuButtonGuide",
            "TowerFall.VariantButton",
            "TowerFall.BladeButton",
            "TowerFall.patch_BladeButton",
            "Monocle.Text",
            "Monocle.OutlineText"
        };

        foreach (var typeName in typeNames)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null) continue;

            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (constructor.GetParameters().Any(parameter => parameter.ParameterType == typeof(string)))
                    yield return constructor;
            }
        }
    }

    private static void Prefix(object[] __args) => TranslationService.TranslateArguments(__args);
}

[HarmonyPatch]
internal static class MenuButtonGuideTextPatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var type = AccessTools.TypeByName("TowerFall.MenuButtonGuide");
        var setDetails = type == null ? null : AccessTools.Method(type, "SetDetails");
        if (setDetails != null && !setDetails.IsAbstract && setDetails.GetMethodBody() != null)
            yield return setDetails;
    }

    private static void Prefix(object[] __args) => TranslationService.TranslateArguments(__args);
}

[HarmonyPatch]
internal static class TextRenderPatch
{
    private static PropertyInfo? drawText;

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var typeName in new[] { "Monocle.Text", "Monocle.OutlineText" })
        {
            var type = AccessTools.TypeByName(typeName);
            var render = type == null ? null : AccessTools.Method(type, "Render");
            if (render != null && !render.IsAbstract && render.GetMethodBody() != null)
                yield return render;
        }
    }

    private static void Prefix(object __instance)
    {
        if (__instance == null) return;

        drawText ??= AccessTools.Property(AccessTools.TypeByName("Monocle.Text"), "DrawText");
        if (drawText?.GetValue(__instance) is string text)
            drawText.SetValue(__instance, TranslationService.Translate(text));
    }
}

[HarmonyPatch]
internal static class ArcherNamePatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var archerData = AccessTools.TypeByName("TowerFall.ArcherData");
        var initialize = archerData == null ? null : AccessTools.Method(archerData, "Initialize");
        if (initialize != null) yield return initialize;
    }

    private static void Postfix() => ApplyTranslations();

    internal static void ApplyTranslations()
    {
        var archerData = AccessTools.TypeByName("TowerFall.ArcherData");
        if (archerData == null) return;

        var name0 = AccessTools.Field(archerData, "Name0");
        var name1 = AccessTools.Field(archerData, "Name1");
        if (name0 == null || name1 == null) return;

        foreach (var propertyName in new[] { "Archers", "AltArchers", "SecretArchers" })
        {
            var property = AccessTools.Property(archerData, propertyName);
            if (property?.GetValue(null) is not IEnumerable archers) continue;

            foreach (var archer in archers)
            {
                if (archer == null) continue;
                if (name0.GetValue(archer) is string first) name0.SetValue(archer, TranslationService.Translate(first));
                if (name1.GetValue(archer) is string second) name1.SetValue(archer, TranslationService.Translate(second));
            }
        }
    }
}

[HarmonyPatch]
internal static class AwardTextPatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var awardInfo = AccessTools.TypeByName("TowerFall.AwardInfo");
        if (awardInfo == null) yield break;

        var drawName = AccessTools.Method(awardInfo, "GetDrawName");
        var archivesName = AccessTools.Method(awardInfo, "GetArchivesName");
        if (drawName != null) yield return drawName;
        if (archivesName != null) yield return archivesName;
    }

    private static void Postfix(ref string[] __result)
    {
        if (__result == null) return;

        var originalTitle = string.Join(" ", __result);
        var translatedTitle = TranslationService.Translate(originalTitle);
        if (translatedTitle == originalTitle)
        {
            for (var index = 0; index < __result.Length; index++)
                __result[index] = TranslationService.Translate(__result[index]);
            return;
        }

        __result = SplitForDisplay(translatedTitle, __result.Length);
    }

    private static string[] SplitForDisplay(string title, int lineCount)
    {
        if (lineCount < 2) return new[] { title };

        var words = title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2) return new[] { title };

        var bestIndex = 1;
        var bestDifference = int.MaxValue;
        for (var index = 1; index < words.Length; index++)
        {
            var firstLength = string.Join(" ", words.Take(index)).Length;
            var secondLength = string.Join(" ", words.Skip(index)).Length;
            var difference = Math.Abs(firstLength - secondLength);
            if (difference < bestDifference)
            {
                bestDifference = difference;
                bestIndex = index;
            }
        }

        return new[]
        {
            string.Join(" ", words.Take(bestIndex)),
            string.Join(" ", words.Skip(bestIndex))
        };
    }
}
