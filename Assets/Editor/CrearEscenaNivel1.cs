using System.IO;
using GuardianesAmazonicos.Nivel1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CrearEscenaNivel1
{
    private const string RutaArte = "Assets/Resources/BienvenidaNivel1_Pantalla.png";

    [MenuItem("Guardianes/Crear escena de bienvenida del Nivel 1")]
    public static void Crear()
    {
        ConfigurarIlustracion();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        escena.name = "BienvenidaNivel1";
        var objeto = new GameObject("Controlador de bienvenida");
        var controlador = objeto.AddComponent<BienvenidaNivel1>();
        var fondo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaArte);
        var audio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BienvenidaNivel1.wav");
        var tipo = typeof(BienvenidaNivel1);
        tipo.GetField("fondoSelva", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, fondo);
        tipo.GetField("audioBienvenida", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audio);
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
