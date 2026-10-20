using System;
using System.Linq;
using GuardianesAmazonicos.Niveles.Nivel01;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrepararEscenasNiveles
{
    private const string RutaEscenas = "Assets/_Project/Scenes";
    private const string RutaEscenaReto = RutaEscenas + "/Nivel01/IdentificarAlJaguar.unity";
    private const string RutaAtlas = "Assets/Sprites/AnimalesSelva_Atlas.png";
    private const string RutaFondo = "Assets/Sprites/BienvenidaSelva.png";
    private const string RutaFuente = "Assets/Fonts/NunitoBoldSDF.asset";

    [MenuItem("Guardianes/Preparar estructura y reto del jaguar")]
    public static void Preparar()
    {
        CrearCarpetasDeNivel();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        CrearEscenaBienvenida.Crear();
        ConfigurarAtlasAnimales();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        CrearEscenaRetoJaguar();
        ConfigurarEscenasDeCompilacion();
        AssetDatabase.SaveAssets();
        Debug.Log("Estructura de cinco niveles y reto del jaguar preparados.");
    }

    private static void CrearCarpetasDeNivel()
    {
        for (var nivel = 1; nivel <= 5; nivel++)
        {
            var nombreCarpeta = $"Nivel{nivel:00}";
            var ruta = $"{RutaEscenas}/{nombreCarpeta}";
            if (!AssetDatabase.IsValidFolder(ruta))
                AssetDatabase.CreateFolder(RutaEscenas, nombreCarpeta);
        }
    }

    private static void ConfigurarAtlasAnimales()
    {
        var importer = AssetImporter.GetAtPath(RutaAtlas) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"No se encontró el atlas de animales en {RutaAtlas}.");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.maxTextureSize = 2048;
        importer.filterMode = FilterMode.Bilinear;
        const float margen = 8f;
        const float ladoCelda = 616f;
        const float origenDerecho = 632f;
        const float origenSuperior = 632f;
#pragma warning disable CS0618 // Unity 6000.6.5f1 aún conserva este importador y serializa correctamente los sub-sprites.
        importer.spritesheet = new[]
        {
            CrearSpriteMeta("Jaguar", new Rect(margen, origenSuperior, ladoCelda, ladoCelda)),
            CrearSpriteMeta("Guacamayo", new Rect(origenDerecho, origenSuperior, ladoCelda, ladoCelda)),
            CrearSpriteMeta("Pecari", new Rect(margen, margen, ladoCelda, ladoCelda)),
            CrearSpriteMeta("Perezoso", new Rect(origenDerecho, margen, ladoCelda, ladoCelda))
        };
#pragma warning restore CS0618
        importer.SaveAndReimport();
    }

    private static SpriteMetaData CrearSpriteMeta(string nombre, Rect rectangulo)
    {
        return new SpriteMetaData
        {
            name = nombre,
            rect = rectangulo,
            alignment = (int)SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        };
    }

    private static void CrearEscenaRetoJaguar()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(RutaAtlas).OfType<Sprite>().ToArray();
        var nombres = new[] { "Jaguar", "Guacamayo", "Pecari", "Perezoso" };
        var imagenes = nombres.Select(nombre => sprites.FirstOrDefault(sprite => sprite.name == nombre)).ToArray();
        if (imagenes.Any(imagen => imagen == null))
        {
            Debug.LogError("El atlas no contiene las cuatro imágenes esperadas para el reto.");
            return;
        }

        var fondo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaFondo);
        var tipografia = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RutaFuente);
        if (fondo == null)
        {
            Debug.LogError("No se encontró el fondo de selva para el reto.");
            return;
        }

        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        escena.name = "IdentificarAlJaguar";
        var objetoControlador = new GameObject("ControladorRetoJaguar");
        var controlador = objetoControlador.AddComponent<ControladorRetoJaguar>();
        var tipo = typeof(ControladorRetoJaguar);
        tipo.GetField("_fondoSelva", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(controlador, fondo);
        tipo.GetField("_tipografia", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(controlador, tipografia);
        tipo.GetField("_imagenesAnimales", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(controlador, imagenes);

        EditorSceneManager.SaveScene(escena, RutaEscenaReto);
    }

    private static void ConfigurarEscenasDeCompilacion()
    {
        const string bienvenida = "Assets/_Project/Scenes/Nivel01/Nivel01_ConozcoLaSelva.unity";
        var escenas = new[]
        {
            new EditorBuildSettingsScene(bienvenida, true),
            new EditorBuildSettingsScene(RutaEscenaReto, true)
        };
        EditorBuildSettings.scenes = escenas;
    }
}
