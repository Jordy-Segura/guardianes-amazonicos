using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;

namespace GuardianesAmazonicos.Presenters
{
    public sealed class BienvenidaNivelPresenter : MonoBehaviour
    {
        [SerializeField] private BienvenidaNivelView _vista;

        private IProgresoRepository _repositorio;
        private AventuraBienvenidaModel _modelo;

        private void Awake()
        {
            if (_vista == null)
            {
                Debug.LogError("BienvenidaNivelPresenter: falta asignar la vista de bienvenida.", this);
                enabled = false;
                return;
            }

            _repositorio = new JsonProgresoRepository();
            ProgresoEstudianteData progreso;

            try
            {
                progreso = _repositorio.Cargar();
            }
            catch (IOException)
            {
                progreso = new ProgresoEstudianteData();
            }
            catch (UnauthorizedAccessException)
            {
                progreso = new ProgresoEstudianteData();
            }

            _modelo = new AventuraBienvenidaModel(progreso);
        }

        private void OnEnable()
        {
            if (_vista == null || _modelo == null)
            {
                return;
            }

            _vista.OnComenzarSolicitado += IniciarAventura;
            _vista.OnAudioSolicitado += ReproducirBienvenida;
            _vista.OnSenasSolicitadas += MostrarSenas;
            _vista.OnCerrarSenasSolicitado += CerrarSenas;
            _vista.OnReproducirSenasSolicitado += ReproducirSenas;
            _modelo.OnProgresoActualizado += ActualizarProgreso;
        }

        private void Start()
        {
            if (_vista != null && _modelo != null)
            {
                _vista.MostrarProgreso(_modelo.PuntoProgresoActual, AventuraBienvenidaModel.TotalPuntosProgreso, _modelo.AventuraIniciada);
            }
        }

        private void OnDisable()
        {
            if (_vista == null || _modelo == null)
            {
                return;
            }

            _vista.OnComenzarSolicitado -= IniciarAventura;
            _vista.OnAudioSolicitado -= ReproducirBienvenida;
            _vista.OnSenasSolicitadas -= MostrarSenas;
            _vista.OnCerrarSenasSolicitado -= CerrarSenas;
            _vista.OnReproducirSenasSolicitado -= ReproducirSenas;
            _modelo.OnProgresoActualizado -= ActualizarProgreso;
        }

        private void IniciarAventura()
        {
            if (_modelo.AventuraIniciada)
            {
                return;
            }

            _modelo.IniciarAventura();

            try
            {
                _repositorio.Guardar(_modelo.Progreso);
                _modelo.NotificarProgresoActualizado();
                Debug.Log("BienvenidaNivelPresenter: aventura iniciada y primer punto de progreso guardado.", this);
            }
            catch (IOException excepcion)
            {
                Debug.LogError("BienvenidaNivelPresenter: no se pudo guardar el progreso. " + excepcion.Message, this);
            }
            catch (UnauthorizedAccessException excepcion)
            {
                Debug.LogError("BienvenidaNivelPresenter: no hay permisos para guardar el progreso. " + excepcion.Message, this);
            }
        }

        private void ReproducirBienvenida()
        {
            _vista.ReproducirBienvenida();
        }

        private void MostrarSenas()
        {
            _vista.MostrarPanelSenas();
        }

        private void CerrarSenas()
        {
            _vista.CerrarPanelSenas();
        }

        private void ReproducirSenas()
        {
            _vista.ReproducirVideoSenas();
        }

        private void ActualizarProgreso(int puntoActual, int totalPuntos)
        {
            _vista.MostrarProgreso(puntoActual, totalPuntos, true);
        }
    }
}
