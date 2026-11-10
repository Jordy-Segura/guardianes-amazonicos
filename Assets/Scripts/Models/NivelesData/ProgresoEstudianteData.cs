using System;
using UnityEngine;

namespace GuardianesAmazonicos.Models
{
    [Serializable]
    public sealed class ProgresoEstudianteData
    {
        public bool bienvenidaSelvaCompletada;
        public bool retoGuacamayoCompletado;
        public bool retoJaguarCompletado;
        public bool retoPlatanoCompletado;
        public bool aventuraIniciada;
        [SerializeField] private bool _retoMonoCuriosoCompletado;
        [SerializeField] private int _progresoNivel2;

        public bool RetoMonoCuriosoCompletado
        {
            get => _retoMonoCuriosoCompletado;
            set => _retoMonoCuriosoCompletado = value;
        }

        public int ProgresoNivel2
        {
            get => _progresoNivel2;
            set => _progresoNivel2 = value;
        }
        public int puntoProgresoActual;
        public int totalPuntosProgreso = 12;
        public string fechaInicioUtc;
    }
}
