using System;

namespace GuardianesAmazonicos.Niveles.Nivel01
{
    public static class ValidadorRespuestaAnimal
    {
        private const string IdentificadorJaguar = "jaguar";

        public static bool EsJaguar(string identificadorAnimal)
        {
            return string.Equals(identificadorAnimal, IdentificadorJaguar, StringComparison.OrdinalIgnoreCase);
        }
    }
}
