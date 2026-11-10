using UnityEngine;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Views
{
    /// <summary>
    /// Reproductor opcional de animaciones por fotogramas PNG (paquete
    /// Materiales_Accesibles_v1). Asigna los PNG importados como Sprite en
    /// orden numerico y reproduce la secuencia sobre un Image de Canvas o un
    /// SpriteRenderer de escena. Empieza detenido y con Reducir movimiento
    /// activo; conecta un Toggle a EstablecerReducirMovimiento para respetar
    /// la opcion de accesibilidad del estudiante.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimacionPersonajePNG : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _destinoEscena;
        [SerializeField] private Image _destinoCanvas;
        [SerializeField] private Sprite[] _fotogramas;
        [SerializeField, Min(1)] private float _imagenesPorSegundo = 16f;
        [SerializeField] private bool _reducirMovimiento = true;

        private bool _reproduciendo;
        private bool _repetir;
        private double _inicio;
        private int _ultimo = -1;

        private void OnEnable()
        {
            _reproduciendo = false;
            _ultimo = -1;
            Mostrar(0);
        }

        private void OnDisable()
        {
            _reproduciendo = false;
        }

        private void Update()
        {
            if (!_reproduciendo || _reducirMovimiento || _fotogramas == null || _fotogramas.Length == 0)
            {
                return;
            }

            double transcurrido = Time.unscaledTimeAsDouble - _inicio;
            int paso = (int)(transcurrido * Mathf.Max(1f, _imagenesPorSegundo));
            if (!_repetir && paso >= _fotogramas.Length)
            {
                _reproduciendo = false;
                Mostrar(0);
                return;
            }

            Mostrar(paso % _fotogramas.Length);
        }

        /// <summary>Reproduce la secuencia una sola vez y vuelve al inicio.</summary>
        public void ReproducirUnCiclo()
        {
            Iniciar(false);
        }

        /// <summary>Reproduce la secuencia en bucle hasta que se pause o detenga.</summary>
        public void ReproducirContinuo()
        {
            Iniciar(true);
        }

        /// <summary>Detiene el avance sin cambiar el fotograma actual.</summary>
        public void Pausar()
        {
            _reproduciendo = false;
        }

        /// <summary>Detiene la animacion y muestra el primer fotograma.</summary>
        public void Detener()
        {
            _reproduciendo = false;
            Mostrar(0);
        }

        /// <summary>Conecta aqui OnValueChanged del Toggle Reducir movimiento.</summary>
        public void EstablecerReducirMovimiento(bool reducir)
        {
            _reducirMovimiento = reducir;
            if (reducir)
            {
                Detener();
            }
        }

        private void Iniciar(bool enBucle)
        {
            if (_reducirMovimiento || _fotogramas == null || _fotogramas.Length == 0)
            {
                return;
            }

            _repetir = enBucle;
            _inicio = Time.unscaledTimeAsDouble;
            _ultimo = -1;
            _reproduciendo = true;
            Mostrar(0);
        }

        private void Mostrar(int indice)
        {
            if (_fotogramas == null || _fotogramas.Length == 0 || indice == _ultimo)
            {
                return;
            }

            _ultimo = indice;
            if (_destinoEscena != null)
            {
                _destinoEscena.sprite = _fotogramas[indice];
            }

            if (_destinoCanvas != null)
            {
                _destinoCanvas.sprite = _fotogramas[indice];
            }
        }
    }
}
