using Microsoft.EntityFrameworkCore;
using PruebaDefontana.Models;
using Serilog;
using Serilog.Core;
using System.Drawing.Printing;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Schema;

namespace PruebaDefontana
{
    public static class Program
    {
        static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-DO", false);

            Console.WriteLine("Hey aqui la prueba de Jonathan Dominguez");
            Console.WriteLine("");

            var ventas = GetVentas(30);
            var detalleVentas = GetDetallesVentas(ventas);

            #region ejercicio 1 total de ventas y cantidad total

            Ejercicio1(ventas);
            
            #endregion

            #region Ejercicio 2. dia y hora de venta mas alta

            Ejercicio2(ventas);

            #endregion

            #region Ejercicio 3. Producto con mayor ventas

            Ejercicio3(detalleVentas);

            #endregion

            #region Ejercicio 4. Local con mas ventas

            Ejercicio4(ventas);

            #endregion

            #region Ejercicio 5. Marca con mayor margen de ganacia

            Ejercicio5(detalleVentas);

            #endregion

            #region Ejercicio 6. Producto mas vendido por local

            Ejercicio6(ventas);

            #endregion
        }


        static List<Ventas> GetVentas(int days)
        {
            using PruebaContext db = new();
            var fecha = DateTime.Now.AddDays(-days);

            return db.Venta.AsQueryable().Where(x => x.Fecha > fecha)
                .Include(x => x.IdLocalNavigation)
                .Include(x=> x.VentaDetalles)
                .ThenInclude(x=>x.IdProductoNavigation)
                .ThenInclude(x=>x.IdMarcaNavigation).ToList();         
        }
        static List<VentaDetalle> GetDetallesVentas(IEnumerable<Ventas> ventas)
        {
            List<VentaDetalle> ventasDetalle = new();
            foreach (var item in ventas)
            {
                ventasDetalle.AddRange(item.VentaDetalles);
            }

            return ventasDetalle;
        }
        static void Ejercicio1(List<Ventas> ventas)
        {
            Console.WriteLine("Ejercicio 1. El total de ventas de los últimos 30 días");
            Console.WriteLine("");

            var ventasTotal = ventas.Sum(x => x.Total).ToString("C2");
            var cantidadTotal = ventas.Sum(x => x.VentaDetalles.Sum(s => s.Cantidad)).ToString("##,#");
            Console.WriteLine($"Monto Total: {ventasTotal}");
            Console.WriteLine($"Cantidad total: {cantidadTotal}");

            Console.WriteLine("");
        }
        static void Ejercicio2 (List<Ventas> ventas)
        {
            Console.WriteLine("Ejercicio 2. dia y hora de venta mas alta");
            Console.WriteLine("");

            var ventaMasAlta = ventas.OrderByDescending(x => x.Total).FirstOrDefault();

            string? dia = ventaMasAlta?.Fecha.ToString("dddd dd MMMM");
            string? hora = ventaMasAlta?.Fecha.ToString("hh:mm:ss tt");
            string? venta = ventaMasAlta?.Total.ToString("C2");
            Console.WriteLine($"Dia:..{dia}");
            Console.WriteLine($"Hora:..{hora}");
            Console.WriteLine($"Venta:.... {venta}");

            Console.WriteLine("");
        }
        static void Ejercicio3 (List<VentaDetalle> detalleVentas)
        {
            Console.WriteLine("Ejercicio 3. Producto con mayor ventas");
            Console.WriteLine("");

            var productoMayorVenta = detalleVentas.GroupBy(x => x.IdProductoNavigation.Nombre)
                .OrderByDescending(x => x.Sum(x => x.IdVentaNavigation.Total)).FirstOrDefault();

            string? MontoVendido = productoMayorVenta?.Sum(x => x.IdVentaNavigation.Total).ToString("C2");
            Console.WriteLine($"Producto:..{productoMayorVenta?.Key}");
            Console.WriteLine($"Monto:..{MontoVendido}");

            Console.WriteLine("");
        }
        static void Ejercicio4 (List<Ventas> ventas)
        {
            Console.WriteLine("Ejercicio 4. Local con mayor ventas");
            Console.WriteLine("");

            var localMayorVenta = ventas.GroupBy(x => x.IdLocalNavigation.Nombre)
                .OrderByDescending(x => x.Sum(x => x.Total)).FirstOrDefault();

            string? Montovendido = localMayorVenta?.Sum(x => x.Total).ToString("C2");
            Console.WriteLine($"Local:..{localMayorVenta?.Key}");
            Console.WriteLine($"Monto:..{Montovendido}");

            Console.WriteLine("");
        }
        static void Ejercicio5(List<VentaDetalle> detalleVentas)
        {
            Console.WriteLine("Ejercicio 5. Marca con mayor margen de ganancia");
            Console.WriteLine("");

            var agrupadoPorMarca = detalleVentas.GroupBy(x => x.IdProductoNavigation.IdMarcaNavigation.Nombre);

            List<(string,int?)> margenGanancia = new();
            int? productMargen = 0;

            foreach (var item in agrupadoPorMarca)
            {
                var groupByProduct = item.GroupBy(x => x.IdProducto);
                foreach (var items in groupByProduct)
                {
                    var costo = (items.Sum(x => x.Cantidad) * item.FirstOrDefault()?.IdProductoNavigation.CostoUnitario);
                    var vendido = (items.Sum(x => x.TotalLinea));
                    var margen = vendido - costo;
                    productMargen += margen;
                }

                margenGanancia.Add((item.Key, productMargen));

                productMargen = 0;
            }

            var mayorMargen = margenGanancia.OrderByDescending(s => s.Item2).FirstOrDefault();
            var marca = mayorMargen.Item1;
            var margenGanacia = mayorMargen.Item2;

            Console.WriteLine($"Marca:..{marca}");
            Console.WriteLine($"Margen:..{margenGanacia?.ToString("C2")}");
            Console.WriteLine("");
        }
        static void Ejercicio6 (List<Ventas> ventas)
        {
            Console.WriteLine("Ejercicio 6. Producto mas vendido por local");
            Console.WriteLine("");

            var agrupadoPorLocal = ventas.GroupBy(x => x.IdLocalNavigation.Nombre);

            foreach (var item in agrupadoPorLocal)
            {
                var detalles = GetDetallesVentas(item);
                var agrupadoPorProducto = detalles.GroupBy(x => x.IdProducto);
                var productoMasVendido = agrupadoPorProducto.OrderByDescending(x => x.Sum(x => x.IdVentaNavigation.Total)).FirstOrDefault();;
                string? producto = productoMasVendido?.FirstOrDefault()?.IdProductoNavigation.Nombre;
                string? monto = productoMasVendido?.Sum(s => s.IdVentaNavigation.Total).ToString("C2");
                Console.WriteLine($"Local:..{item.Key}");
                Console.WriteLine($"Producto:..{producto}");
                Console.WriteLine($"Monto:..{monto}");
                Console.WriteLine("");
            }
        }
    }
}