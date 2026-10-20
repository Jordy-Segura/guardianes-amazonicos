using System.IO;
using GuardianesAmazonicos.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using GuardianesAmazonicos.Data;

public static class CrearEscenaBienvenida
{
    private const string RutaEscena = "Assets/_Project/Scenes/Nivel01/Nivel01_ConozcoLaSelva.unity";
    public const string RutaArte = "Assets/Sprites/BienvenidaSelva.png";
    private const string RutaFuente = "Assets/Fonts/NunitoBold.ttf";
    private const string RutaFuenteTMP = "Assets/Fonts/NunitoBoldSDF.asset";

    [MenuItem("Guardianes/Crear escena de bienvenida")]
    public static void Crear()
    {
        ConfigurarIlustracion();
        ConfigurarTipografia();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        escena.name = "Nivel01_ConozcoLaSelva";
        CrearCamaraPrincipal();
        var objeto = new GameObject("Controlador de bienvenida");
        var controlador = objeto.AddComponent<ControladorNivel>();
        var fondo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaArte);
        var audio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Bienvenida.wav");
        var audioInicio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel1Inicio.wav");
        var audioPista = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/PistaNivel1.wav");
        var audioContinuacion = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ContinuacionNivel1.wav");
        var tipografia = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RutaFuenteTMP);
        var tipo = typeof(ControladorNivel);
        tipo.GetField("_fondoSelva", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, fondo);
        tipo.GetField("_audioBienvenida", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audio);
        tipo.GetField("_audioInicio", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audioInicio);
        tipo.GetField("_audioPista", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audioPista);
        tipo.GetField("_audioContinuacion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audioContinuacion);
        tipo.GetField("_tipografia", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, tipografia);
        EditorSceneManager.SaveScene(escena, RutaEscena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Escena de bienvenida creada y añadida a Build Settings.");
    }

    private static void CrearCamaraPrincipal()
    {
        var objetoCamara = new GameObject("Main Camera");
        objetoCamara.tag = "MainCamera";
        objetoCamara.transform.position = new Vector3(0f, 0f, -10f);
        var camara = objetoCamara.AddComponent<Camera>();
        camara.orthographic = true;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.08f, 0.29f, 0.19f);
        objetoCamara.AddComponent<AudioListener>();
    }

    [MenuItem("Guardianes/Reiniciar progreso local")]
    public static void ReiniciarProgreso()
    {
        GuardadoProgreso.Reiniciar();
        Debug.Log("Progreso local reiniciado.");
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
        fuenteTmp.TryAddCharacters("¡Hola, explorador! Bienvenido a la selva. Hoy vamos a conocer algunos de sus tesoros. ¿Quieres comenzar? NIVEL 1 CONOZCO progreso ayuda señas lectura AUDIO ESCUCHAR COMENZAR PISTA LENGUA DE SEÑAS ECUATORIANA Video en preparación REPRODUCIR REPETIR AMPLIAR VISOR VOLVER A LA ACTIVIDAD Aprende a tu ritmo …");
        EditorUtility.SetDirty(fuenteTmp);
        AssetDatabase.SaveAssets();
    }
}

public sealed class BienvenidaSpriteImportador : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath != CrearEscenaBienvenida.RutaArte)
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
        const string ruta = CrearEscenaBienvenida.RutaArte;
        var importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null || importer.textureType == TextureImporterType.Sprite)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }
}
