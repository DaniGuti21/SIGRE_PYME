namespace SIGRE_PYME.Models
{
    public class CarritoItem
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
    }
}