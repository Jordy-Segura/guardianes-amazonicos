using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Views
{
    public sealed class BienvenidaNivelView : MonoBehaviour, IBienvenidaView
    {
        [Header("Contenido")]
        [SerializeField] private TMP_Text _textoBienvenida;
        [SerializeField] private TMP_Text _textoProgreso;
        [SerializeField] private TMP_Text _textoEstadoAudio;
        [SerializeField] private TMP_Text _textoEstadoSenas;
        [SerializeField] private TMP_Text _textoComenzar;

        [Header("Controles")]
        [SerializeField] private Button _botonComenzar;
        [SerializeField] private Button _botonAudio;
        [SerializeField] private Button _botonSenas;
        [SerializeField] private Button _botonCerrarSenas;
        [SerializeField] private Button _botonReproducirSenas;

        [Header("Canales multimodales")]
        [SerializeField] private AudioSource _fuenteAudio;
        [SerializeField] private VideoPlayer _reproductorSenas;
        [SerializeField] private GameObject _panelSenas;

        private Coroutine _actualizarEstadoAudio;

        public event Action OnComenzarSolicitado;
        public event Action OnAudioSolicitado;
        public event Action OnSenasSolicitadas;
        public event Action OnCerrarSenasSolicitado;
        public event Action OnReproducirSenasSolicitado;

        private void OnEnable()
        {
            if (_botonComenzar != null) _botonComenzar.onClick.AddListener(EmitirComenzar);
            if (_botonAudio != null) _botonAudio.onClick.AddListener(EmitirAudio);
            if (_botonSenas != null) _botonSenas.onClick.AddListener(EmitirSenas);
            if (_botonCerrarSenas != null) _botonCerrarSenas.onClick.AddListener(EmitirCerrarSenas);
            if (_botonReproducirSenas != null) _botonReproducirSenas.onClick.AddListener(EmitirReproducirSenas);
        }

        private void OnDisable()
        {
            if (_botonComenzar != null) _botonComenzar.onClick.RemoveListener(EmitirComenzar);
            if (_botonAudio != null) _botonAudio.onClick.RemoveListener(EmitirAudio);
            if (_botonSenas != null) _botonSenas.onClick.RemoveListener(EmitirSenas);
            if (_botonCerrarSenas != null) _botonCerrarSenas.onClick.RemoveListener(EmitirCerrarSenas);
            if (_botonReproducirSenas != null) _botonReproducirSenas.onClick.RemoveListener(EmitirReproducirSenas);

            if (_fuenteAudio != null && _fuenteAudio.isPlaying)
            {
                _fuenteAudio.Stop();
            }

            if (_reproductorSenas != null && _reproductorSenas.isPlaying)
            {
                _reproductorSenas.Stop();
            }
        }

        public void MostrarProgreso(int puntoActual, int totalPuntos, bool aventuraIniciada)
        {
            if (_textoProgreso != null)
            {
                _textoProgreso.text = aventuraIniciada
                    ? "PROGRESO  " + puntoActual + "  DE  " + totalPuntos
                    : "PROGRESO  0  DE  " + totalPuntos;
            }

            if (_textoComenzar != null)
            {
                _textoComenzar.text = aventuraIniciada ? "¡AVENTURA INICIADA!" : "¡COMENZAR!";
            }

            if (_botonComenzar != null)
            {
                _botonComenzar.interactable = !aventuraIniciada;
            }
        }

        public void ReproducirBienvenida()
        {
            if (_fuenteAudio == null || _fuenteAudio.clip == null)
            {
                MostrarEstadoAudio("La locución de bienvenida aún no está disponible.");
                return;
            }

            if (_fuenteAudio.isPlaying)
            {
                _fuenteAudio.Stop();
            }

            _fuenteAudio.Play();
            MostrarEstadoAudio("Reproduciendo la bienvenida...");

            if (_actualizarEstadoAudio != null)
            {
                StopCoroutine(_actualizarEstadoAudio);
            }

            _actualizarEstadoAudio = StartCoroutine(EsperarFinDeAudio());
        }

        public void MostrarPanelSenas()
        {
            if (_panelSenas != null)
            {
                _panelSenas.SetActive(true);
                _panelSenas.transform.SetAsLastSibling();
            }

            bool tieneVideo = _reproductorSenas != null && _reproductorSenas.clip != null;
            if (_botonReproducirSenas != null)
            {
                _botonReproducirSenas.interactable = tieneVideo;
            }

            if (_textoEstadoSenas != null)
            {
                _textoEstadoSenas.text = tieneVideo
                    ? "Bienvenida disponible en LSEC."
                    : "Video de bienvenida en LSEC pendiente de grabación. La ventana reproducirá un archivo local cuando se incorpore.";
            }
        }

        public void CerrarPanelSenas()
        {
            if (_reproductorSenas != null && _reproductorSenas.isPlaying)
            {
                _reproductorSenas.Stop();
            }

            if (_panelSenas != null)
            {
                _panelSenas.SetActive(false);
            }
        }

        public void ReproducirVideoSenas()
        {
            if (_reproductorSenas == null || _reproductorSenas.clip == null)
            {
                MostrarPanelSenas();
                return;
            }

            _reproductorSenas.Stop();
            _reproductorSenas.Play();
        }

        private IEnumerator EsperarFinDeAudio()
        {
            while (_fuenteAudio != null && _fuenteAudio.isPlaying)
            {
                yield return null;
            }

            MostrarEstadoAudio("Puedes volver a escuchar la bienvenida cuando quieras.");
            _actualizarEstadoAudio = null;
        }

        private void MostrarEstadoAudio(string mensaje)
        {
            if (_textoEstadoAudio != null)
            {
                _textoEstadoAudio.text = mensaje;
            }
        }

        private void EmitirComenzar() { OnComenzarSolicitado?.Invoke(); }
        private void EmitirAudio() { OnAudioSolicitado?.Invoke(); }
        private void EmitirSenas() { OnSenasSolicitadas?.Invoke(); }
        private void EmitirCerrarSenas() { OnCerrarSenasSolicitado?.Invoke(); }
        private void EmitirReproducirSenas() { OnReproducirSenasSolicitado?.Invoke(); }
    }
}
