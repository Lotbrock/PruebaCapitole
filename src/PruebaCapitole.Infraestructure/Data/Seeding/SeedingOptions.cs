namespace PruebaCapitole.Infrastructure.Data.Seeding
{
    /// <summary>Se lee de la sección "Seeding" de appsettings.</summary>
    public sealed class SeedingOptions
    {
        public const string SectionName = "Seeding";

        /// <summary>Si es false, no se insertan datos de ejemplo.</summary>
        public bool Enabled { get; set; }

        /// <summary>Cuántas reservas aleatorias generar (se suman a las de casos especiales).</summary>
        public int BookingsCount { get; set; } = 50;

        /// <summary>Misma semilla = mismos datos en cada ejecución.</summary>
        public int RandomSeed { get; set; } = 12345;
    }
}
