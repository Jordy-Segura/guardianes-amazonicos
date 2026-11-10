using UnityEngine;

namespace GuardianesAmazonicos.Views
{
    public sealed class AnimacionEntradaUI : MonoBehaviour
    {
        private Vector3 _escalaObjetivo;
        private Vector2 _posicionObjetivo;
        private float _tiempo;
        private bool _correcto = true;

        private void OnEnable()
        {
            _escalaObjetivo = transform.localScale;
            RectTransform rect = transform as RectTransform;
            if (rect != null) _posicionObjetivo = rect.anchoredPosition;
            transform.localScale = _escalaObjetivo * 0.82f;
            _tiempo = 0f;
        }

        public void Reproducir(bool correcto)
        {
            _correcto = correcto;
            _tiempo = 0f;
            transform.localScale = _escalaObjetivo * 0.82f;
            RectTransform rect = transform as RectTransform;
            if (rect != null) rect.anchoredPosition = _posicionObjetivo;
        }

        private void Update()
        {
            if (PlayerPrefs.GetInt("HU03_MovimientoReducido", 0) == 1)
            {
                transform.localScale = _escalaObjetivo;
                RectTransform movimientoReducido = transform as RectTransform;
                if (movimientoReducido != null) movimientoReducido.anchoredPosition = _posicionObjetivo;
                return;
            }
            _tiempo += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(_tiempo / 0.34f);
            float amplitud = _correcto ? 0.14f : 0.06f;
            float rebote = 1f + amplitud * Mathf.Sin(progreso * Mathf.PI);
            transform.localScale = _escalaObjetivo * Mathf.Lerp(0.82f, rebote, progreso);
            RectTransform rect = transform as RectTransform;
            if (rect != null)
            {
                float desplazamiento = _correcto ? 0f : Mathf.Sin(_tiempo * 54f) * 12f * (1f - progreso);
                rect.anchoredPosition = _posicionObjetivo + Vector2.right * desplazamiento;
            }
        }
    }
}
