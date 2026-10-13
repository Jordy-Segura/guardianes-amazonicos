namespace GuardianesAmazonicos.Nivel1.HU03
{
    public static class GuacamayoContent
    {
        public const string Introduction = "Soy el Guacamayo. Escucha mi nombre y encuentra mi imagen.";
        public const string Question = "¿Dónde está el guacamayo?";
        public const string Hint = "Busca el ave de plumas rojas, amarillas y azules. Tiene pico curvo y una cola larga.";
        public const string Success = "¡Excelente! Encontraste al guacamayo.";
        public const string Retry = "Puedes intentarlo otra vez. Mira la pista y busca al guacamayo.";
        public const string MissingSigns = "Video en preparación";

        public static string SignsText(string context)
        {
            switch (context)
            {
                case "Pista": return Hint;
                case "Error": return "Inténtalo otra vez. " + Hint;
                case "Acierto": return Success;
                case "Cierre": return "¡Reto completado! Tu acierto está guardado. Próximo reto: las plantas de la selva.";
                case "Respuestas": return Question + " Opciones: mono, guacamayo y tortuga.";
                default: return Introduction;
            }
        }
    }
}
