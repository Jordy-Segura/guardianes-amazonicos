using UnityEngine;
using UnityEngine.UI;

namespace GuardianesAmazonicos.MaterialesAccesiblesDemo
{

// Reproductor opcional: asigna los PNG importados como Sprite en orden numérico.
[DisallowMultipleComponent]
public class AnimacionPersonajePNG : MonoBehaviour
{
    [SerializeField] private SpriteRenderer destinoEscena;
    [SerializeField] private Image destinoCanvas;
    [SerializeField] private Sprite[] fotogramas;
    [SerializeField, Min(1)] private float imagenesPorSegundo = 16f;
    [SerializeField] private bool reducirMovimiento = true;
    private bool reproduciendo;
    private bool repetir;
    private double inicio;
    private int ultimo = -1;

    private void OnEnable() { reproduciendo = false; ultimo = -1; Mostrar(0); }
    private void OnDisable() { reproduciendo = false; }

    private void Update()
    {
        if (!reproduciendo || reducirMovimiento || fotogramas == null || fotogramas.Length == 0) return;
        double transcurrido = Time.unscaledTimeAsDouble - inicio;
        int paso = (int)(transcurrido * Mathf.Max(1f, imagenesPorSegundo));
        if (!repetir && paso >= fotogramas.Length) { reproduciendo = false; Mostrar(0); return; }
        Mostrar(paso % fotogramas.Length);
    }

    public void ReproducirUnCiclo() { Iniciar(false); }
    public void ReproducirContinuo() { Iniciar(true); }
    public void Pausar() { reproduciendo = false; }
    public void Detener() { reproduciendo = false; Mostrar(0); }

    // Conecta aquí OnValueChanged del Toggle «Reducir movimiento».
    public void EstablecerReducirMovimiento(bool reducir)
    {
        reducirMovimiento = reducir;
        if (reducir) Detener();
    }

    private void Iniciar(bool enBucle)
    {
        if (reducirMovimiento || fotogramas == null || fotogramas.Length == 0) return;
        repetir = enBucle; inicio = Time.unscaledTimeAsDouble; ultimo = -1;
        reproduciendo = true; Mostrar(0);
    }

    private void Mostrar(int indice)
    {
        if (fotogramas == null || fotogramas.Length == 0 || indice == ultimo) return;
        ultimo = indice;
        if (destinoEscena != null) destinoEscena.sprite = fotogramas[indice];
        if (destinoCanvas != null) destinoCanvas.sprite = fotogramas[indice];
    }
}

}