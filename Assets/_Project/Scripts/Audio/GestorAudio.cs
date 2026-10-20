using UnityEngine;

namespace GuardianesAmazonicos.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class GestorAudio : MonoBehaviour
    {
        private AudioSource _fuente;

        public void Configurar()
        {
            _fuente = GetComponent<AudioSource>();
            _fuente.playOnAwake = false;
            _fuente.spatialBlend = 0f;
        }

        public bool Reproducir(AudioClip clip)
        {
            if (clip == null)
                return false;

            if (_fuente == null)
                Configurar();

            _fuente.Stop();
            _fuente.clip = clip;
            _fuente.Play();
            return true;
        }

        public void Detener()
        {
            if (_fuente != null && _fuente.isPlaying)
                _fuente.Stop();
        }
    }
}
