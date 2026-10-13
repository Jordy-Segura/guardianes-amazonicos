using System;
using System.IO;
using System.Linq;
using GuardianesAmazonicos.Nivel1.HU03;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class GuacamayoBuild
{
    public const string ScenePath = "Assets/Scenes/GuacamayoNivel1.unity";

    [MenuItem("Guardianes/HU-03/Preparar escena")]
    public static void Prepare()
    {
        foreach (string path in Directory.GetFiles("Assets/Resources/HU03", "*.png"))
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        CreateFont();
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("HU03_Guacamayo", typeof(GuacamayoScreen));
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.All(scene => scene.path != ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void CreateFont()
    {
        const string fontPath = "Assets/Resources/HU03/NunitoExtraBold SDF.asset";
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath) != null) return;
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NunitoExtraBold.ttf");
        Require(source != null, "Nunito ExtraBold no encontrada");
        var font = TMP_FontAsset.CreateFontAsset(source, 70, 8, GlyphRenderMode.SDFAA, 2048, 2048,
            AtlasPopulationMode.Dynamic, true);
        font.name = "NunitoExtraBold SDF";
        string glyphs = string.Concat(Enumerable.Range(32, 224).Select(code => (char)code));
        font.TryAddCharacters(glyphs, out string missing);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font, fontPath);
        font.material.name = font.name + " Material";
        AssetDatabase.AddObjectToAsset(font.material, font);
        foreach (var texture in font.atlasTextures)
        {
            texture.name = font.name + " Atlas";
            AssetDatabase.AddObjectToAsset(texture, font);
        }
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Guardianes/HU-03/Validar criterios")]
    public static void Validate()
    {
        var activity = new GuacamayoActivity();
        int completed = 0;
        int continued = 0;
        activity.Completed += () => completed++;
        activity.ContinueRequested += () => continued++;
        activity.Continue();
        Require(continued == 0, "No se puede avanzar antes de acertar");
        Require(!activity.Choose(AnimalChoice.Guacamayo), "No se responde antes de mostrar opciones");
        activity.ShowAnswers();
        Require(!activity.Choose(AnimalChoice.Mono), "Mono debe permitir reintento");
        Require(!activity.Choose(AnimalChoice.Tortuga), "Tortuga debe permitir reintento");
        Require(activity.Stage == ActivityStage.Answers && completed == 0, "Error no completa la actividad");
        Require(activity.Choose(AnimalChoice.Guacamayo), "Guacamayo es la respuesta correcta");
        Require(activity.CanContinue && completed == 1, "Acierto habilita continuar");
        activity.Choose(AnimalChoice.Guacamayo);
        Require(completed == 1, "Doble clic no duplica el guardado");
        activity.Continue();
        activity.Continue();
        Require(continued == 1 && activity.Stage == ActivityStage.Finished, "Continuar se emite una sola vez");
        Require(GuacamayoProgress.AdvanceContiguousProgress(0) == 0, "No se inventa progreso de historias anteriores");
        Require(GuacamayoProgress.AdvanceContiguousProgress(2) == 3, "Progreso 2 avanza a 3");
        Require(GuacamayoProgress.AdvanceContiguousProgress(9) == 9, "No disminuye progreso posterior");
        foreach (string name in new[] { "Instruccion", "Palabra", "Pista", "Error", "Acierto", "Cierre" })
            Require(Resources.Load<AudioClip>("HU03/Audio/" + name) != null, "Audio local requerido: " + name);
        foreach (string name in new[] { "FondoSelva", "Mono", "Guacamayo", "Tortuga" })
            Require(Resources.Load<Sprite>("HU03/" + name) != null, "Sprite requerido: " + name);
        Require(File.Exists("Assets/Scenes/BienvenidaNivel1.unity"), "Se conserva HU-01");
        Directory.CreateDirectory("Builds/Validation");
        File.WriteAllText("Builds/Validation/HU03-editmode.txt",
            "PASS: estados, 2 distractores, acierto, doble clic, avance, progreso idempotente, 6 audios, 4 sprites y HU-01.\n");
        Debug.Log("HU03_EDITMODE_PASS");
    }

    [MenuItem("Guardianes/HU-03/Compilar Windows")]
    public static void BuildRelease()
    {
        Build(false);
    }

    public static void BuildSmoke()
    {
        Build(true);
    }

    [MenuItem("Guardianes/HU-03/Configurar VS Code")]
    public static void ConfigureVsCode()
    {
        string editorPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "Microsoft VS Code", "Code.exe");
        if (!File.Exists(editorPath))
        {
            Debug.Log("Selecciona tu editor en Edit > Preferences > External Tools.");
            return;
        }
        Unity.CodeEditor.CodeEditor.SetExternalScriptEditor(editorPath);
        Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
    }

    public static void BuildDelivery()
    {
        ConfigureVsCode();
        BuildSmoke();
        BuildRelease();
    }

    private static void Build(bool development)
    {
        Prepare();
        string folder = development ? "Builds/HU03-Smoke" : "Builds/HU03-Windows";
        Directory.CreateDirectory(folder);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath, "Assets/Scenes/BienvenidaNivel1.unity" },
            locationPathName = folder + "/Guardianes-HU03.exe",
            target = BuildTarget.StandaloneWindows64,
            extraScriptingDefines = development ? new[] { "HU03_ACCEPTANCE" } : Array.Empty<string>(),
            options = development ? BuildOptions.Development : BuildOptions.None
        });
        Require(report.summary.result == BuildResult.Succeeded, "Falló la compilación Windows");
        Debug.Log("HU03_BUILD_PASS: " + folder);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("HU03: " + message);
    }
}
