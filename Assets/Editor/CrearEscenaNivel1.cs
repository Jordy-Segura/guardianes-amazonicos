using System.IO;
using GuardianesAmazonicos.Nivel1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

public static class CrearEscenaNivel1
{
    private const string RutaArte = "Assets/Resources/BienvenidaNivel1_Pantalla.png";
    private const string RutaFuente = "Assets/Fonts/NunitoBold.ttf";
    private const string RutaFuenteTMP = "Assets/Resources/NunitoBoldSDF.asset";

    [MenuItem("Guardianes/Crear escena de bienvenida del Nivel 1")]
    public static void Crear()
    {
        ConfigurarIlustracion();
        ConfigurarTipografia();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        escena.name = "BienvenidaNivel1";
        var objeto = new GameObject("Controlador de bienvenida");
        var controlador = objeto.AddComponent<BienvenidaNivel1>();
        var fondo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaArte);
        var audio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BienvenidaNivel1.wav");
        var tipo = typeof(BienvenidaNivel1);
        tipo.GetField("_fondoSelva", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, fondo);
        tipo.GetField("_audioBienvenida", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audio);
        EditorSceneManager.SaveScene(escena, "Assets/Scenes/BienvenidaNivel1.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/BienvenidaNivel1.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Escena de bienvenida del Nivel 1 creada y añadida a Build Settings.");
    }

    [MenuItem("Guardianes/Reiniciar progreso local del Nivel 1")]
    public static void ReiniciarProgreso()
    {
        PlayerPrefs.DeleteKey("guardianes.nivel1.iniciado");
        PlayerPrefs.DeleteKey("guardianes.nivel1.progreso");
        PlayerPrefs.Save();
        Debug.Log("Progreso local del Nivel 1 reiniciado.");
    }

    private static void ConfigurarIlustracion()
    {
        var importer = AssetImporter.GetAtPath(RutaArte) as TextureImporter;
        if (importer == null)
            return;
        if (importer.textureType == TextureImporterType.Sprite)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void ConfigurarTipografia()
    {
        var fuenteTmpExistente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RutaFuenteTMP);
        if (fuenteTmpExistente != null)
            return;

        var fuente = AssetDatabase.LoadAssetAtPath<Font>(RutaFuente);
        if (fuente == null)
        {
            Debug.LogWarning("No se encontró la fuente Nunito; se usará la tipografía de TextMeshPro.");
            return;
        }

        var fuenteTmp = TMP_FontAsset.CreateFontAsset(fuente, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        if (fuenteTmp == null)
        {
            Debug.LogWarning("No se pudo crear la fuente TextMeshPro de Nunito.");
            return;
        }

        fuenteTmp.name = "NunitoSDF";
        AssetDatabase.CreateAsset(fuenteTmp, RutaFuenteTMP);
        foreach (var atlas in fuenteTmp.atlasTextures)
            AssetDatabase.AddObjectToAsset(atlas, fuenteTmp);
        AssetDatabase.AddObjectToAsset(fuenteTmp.material, fuenteTmp);
        fuenteTmp.TryAddCharacters("¡Hola, explorador! Bienvenido a la selva. Hoy vamos a conocer algunos de sus tesoros. ¿Quieres comenzar? NIVEL 1 CONOZCO progreso ayuda señas lectura AUDIO ESCUCHAR COMENZAR PISTA LENGUA DE SEÑAS ECUATORIANA Video en preparación REPRODUCIR REPETIR AMPLIAR VISOR VOLVER A LA ACTIVIDAD Aprende a tu ritmo");
        EditorUtility.SetDirty(fuenteTmp);
        AssetDatabase.SaveAssets();
    }
}

public sealed class BienvenidaSpriteImportador : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath != "Assets/Resources/BienvenidaNivel1_Pantalla.png")
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.maxTextureSize = 2048;
        importer.alphaIsTransparency = false;
    }
}

[InitializeOnLoad]
public static class ConfigurarArteBienvenida
{
    static ConfigurarArteBienvenida()
    {
        EditorApplication.delayCall += Configurar;
    }

    private static void Configurar()
    {
        const string ruta = "Assets/Resources/BienvenidaNivel1_Pantalla.png";
        var importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null || importer.textureType == TextureImporterType.Sprite)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }
}
