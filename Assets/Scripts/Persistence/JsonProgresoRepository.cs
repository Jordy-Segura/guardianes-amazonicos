using System;
using System.IO;
using GuardianesAmazonicos.Models;
using UnityEngine;

namespace GuardianesAmazonicos.Persistence
{
    public sealed class JsonProgresoRepository : IProgresoRepository
    {
        private const string NombreArchivo = "progreso.json";

        private string RutaArchivo
        {
            get { return Path.Combine(Application.persistentDataPath, NombreArchivo); }
        }

        public ProgresoEstudianteData Cargar()
        {
            if (!File.Exists(RutaArchivo))
            {
                return new ProgresoEstudianteData();
            }

            try
            {
                string contenido = File.ReadAllText(RutaArchivo);
                ProgresoEstudianteData progreso = JsonUtility.FromJson<ProgresoEstudianteData>(contenido);
                return progreso ?? new ProgresoEstudianteData();
            }
            catch (IOException excepcion)
            {
                Debug.LogError("JsonProgresoRepository: no se pudo leer el progreso local. " + excepcion.Message);
                throw;
            }
            catch (UnauthorizedAccessException excepcion)
            {
                Debug.LogError("JsonProgresoRepository: no hay permisos para leer el progreso local. " + excepcion.Message);
                throw;
            }
        }

        public void Guardar(ProgresoEstudianteData progreso)
        {
            if (progreso == null)
            {
                throw new ArgumentNullException(nameof(progreso));
            }

            string carpeta = Application.persistentDataPath;
            Directory.CreateDirectory(carpeta);

            string temporal = RutaArchivo + ".tmp";
            string contenido = JsonUtility.ToJson(progreso, true);
            File.WriteAllText(temporal, contenido);

            if (File.Exists(RutaArchivo))
            {
                File.Replace(temporal, RutaArchivo, null);
            }
            else
            {
                File.Move(temporal, RutaArchivo);
            }
        }
    }
}
