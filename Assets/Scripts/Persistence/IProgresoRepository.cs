using GuardianesAmazonicos.Models;

namespace GuardianesAmazonicos.Persistence
{
    public interface IProgresoRepository
    {
        ProgresoEstudianteData Cargar();
        void Guardar(ProgresoEstudianteData progreso);
    }
}
