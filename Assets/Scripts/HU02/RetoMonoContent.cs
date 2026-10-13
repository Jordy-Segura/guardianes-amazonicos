namespace GuardianesAmazonicos.Nivel1.HU02
{
    public static class RetoMonoContent
    {
        public const string Introduction = "¡Mira! En la selva viven muchos animales. ¿Puedes encontrar al mono?";
        public const string Question = "¿Dónde está el mono?";
        public const string Hint = "Busca al animal de pelo café y cola larga que vive en los árboles.";
        public const string Success = "¡Muy bien! Ese es el mono.";
        public const string Retry = "Puedes intentarlo otra vez. Mira la pista y busca al mono.";
        public const string MissingSigns = "Video en preparación";

        public static string SignsText(string context)
        {
            switch (context)
            {
                case "Pista": return Hint;
                case "Error": return "Inténtalo otra vez. " + Hint;
                case "Acierto": return Success;
                case "Cierre": return "¡Reto completado! Tu acierto está guardado. Puedes volver al inicio.";
                case "Respuestas": return Question + " Opciones: mono, guacamayo y tortuga.";
                default: return Introduction;
            }
        }
    }
}
