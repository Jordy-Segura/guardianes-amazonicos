using GuardianesAmazonicos.Nivel1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CrearEscenaNivel1
{
    [MenuItem("Guardianes/Crear escena de bienvenida del Nivel 1")]
    public static void Crear()
    {
        ConfigurarIlustracion();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        escena.name = "BienvenidaNivel1";
        var objeto = new GameObject("Controlador de bienvenida");
        var controlador = objeto.AddComponent<BienvenidaNivel1>();
        var fondo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/BienvenidaSelva.png");
        var audio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BienvenidaNivel1.wav");
        var tipo = typeof(BienvenidaNivel1);
        tipo.GetField("fondoSelva", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, fondo);
        tipo.GetField("audioBienvenida", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(controlador, audio);
        EditorSceneManager.SaveScene(escena, "Assets/Scenes/BienvenidaNivel1.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/BienvenidaNivel1.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Escena BienvenidaNivel1 creada y añadida a Build Settings.");
    }

    private static void ConfigurarIlustracion()
    {
        var importer = AssetImporter.GetAtPath("Assets/Art/BienvenidaSelva.png") as TextureImporter;
        if (importer == null) return;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}
