namespace SIGRE_PYME.Helpers
{
    public static class RolesSistema
    {
        public const int AdminId = 1;
        public const int VendedorId = 2;
        public const int AlmacenistaId = 3;
        public const int GerenteId = 4;
        public const int ClienteId = 5;

        public const string Admin = "Admin";
        public const string Vendedor = "Vendedor";
        public const string Almacenista = "Almacenista";
        public const string Gerente = "Gerente";
        public const string Cliente = "Cliente";

        public static readonly Dictionary<int, string> Roles = new()
        {
            { AdminId, Admin },
            { VendedorId, Vendedor },
            { AlmacenistaId, Almacenista },
            { GerenteId, Gerente },
            { ClienteId, Cliente }
        };

        public static string ObtenerNombre(int rolId)
        {
            return Roles.TryGetValue(rolId, out var nombre) ? nombre : "SinRol";
        }

        public static bool EsValido(int rolId)
        {
            return Roles.ContainsKey(rolId);
        }
    }
}