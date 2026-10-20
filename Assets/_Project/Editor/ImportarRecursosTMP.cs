using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ImportarRecursosTMP
{
    private const string RutaConfiguracionTMP = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
    private const string RutaPaqueteTMP = "Resources/PackageManager/BuiltInPackages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

    static ImportarRecursosTMP()
    {
        EditorApplication.delayCall += ImportarSiHaceFalta;
        EditorApplication.playModeStateChanged += AlCambiarModoDeJuego;
    }

    private static void AlCambiarModoDeJuego(PlayModeStateChange estado)
    {
        if (estado == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += ImportarSiHaceFalta;
    }

    private static void ImportarSiHaceFalta()
    {
        if (EditorApplication.isPlaying || File.Exists(RutaConfiguracionTMP))
            return;

        var paquete = Path.Combine(EditorApplication.applicationContentsPath, RutaPaqueteTMP);
        if (!File.Exists(paquete))
        {
            Debug.LogWarning("No se encontraron los recursos esenciales de TextMesh Pro en esta instalación de Unity. Impórtalos desde Window > TextMesh Pro > Import TMP Essential Resources.");
            return;
        }

        Debug.Log("Importando automáticamente los recursos esenciales de TextMesh Pro para este proyecto.");
        UnityEditor.AssetPackage.Package.Import(paquete, false);
    }
}
