using System;
using System.Globalization;
using System.Text;

public class CiudadOptimaCalculator
{
    public class Ciudad
    {
        public string Nombre { get; set; }
        public Dictionary<string, double> Distancias { get; set; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    }
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("CALCULADORA DE CIUDAD ÓPTIMA");
            Console.WriteLine("=========================================");
            Console.WriteLine("\nMenu Principal:");
            Console.WriteLine("1. Calcular ciudad optima");
            Console.WriteLine("2. Salir");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.WriteLine("Ingrese la ruta del archivo");
                    string rutaArchivo = Console.ReadLine();
                    Calcular(rutaArchivo);
                    break;
                case "2":
                    return;
                default:
                    Console.WriteLine("\nOpcion invalida o no disponible");
                    break;
            }
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }

    private static void Calcular(string archivo)
    {
        if (!File.Exists(archivo))
        {
            Console.WriteLine($"error: No se encontró el archivo {archivo}");
            return;
        }

        var ciudades = LeerCSV(archivo);
        Console.WriteLine("\nCiudades disponibles:");
        foreach (var ciudad in ciudades)
        {
            Console.WriteLine($"- {ciudad.Key}");
        }

        Console.WriteLine("\nIngresa las ciudades que desees analizar (separadas por coma):");
        string input = Console.ReadLine();
        var ciudadesSeleccionadas = string.IsNullOrWhiteSpace(input)
            ? ciudades.Keys.ToList()
            : input.Split(',').Select(c => c.Trim()).ToList();
        var resultados = CalcularDistanciasPromedio(ciudades, ciudadesSeleccionadas);

        Console.WriteLine("\nResultados:");
        foreach (var resultado in resultados.OrderBy(r => r.Promedio))
        {
            Console.WriteLine($"{resultado.Ciudad,-20}: Distancia promedio = {resultado.Promedio,8:F2} km");
        }

        if (resultados.Any())
        {
            var ciudadOptima = resultados.OrderBy(r => r.Promedio).First();
            Console.WriteLine($"\nCIUDAD OPTIMA: {ciudadOptima.Ciudad} con distancia promedio de {ciudadOptima.Promedio:F2} km");
        }
        else
        {
            Console.WriteLine("\nNo hay resultados");
        }
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return texto;
        }
        string Stringnormalizado = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char c in Stringnormalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }

    private static Dictionary<string, Ciudad> LeerCSV(string filePath)
    {
        var ciudades = new Dictionary<string, Ciudad>();
        var lineas = File.ReadAllLines(filePath);

        if (lineas.Length == 0) return ciudades;
        var encabezados = lineas[0].Split(',').Select(x => x.Trim('"')).ToList();

        for (int i = 1; i < lineas.Length; i++)
        {
            var valores = lineas[i].Split(',').Select(x => x.Trim('"')).ToList();
            if (valores.Count < 2 || string.IsNullOrEmpty(valores[0])) continue;

            string ciudadOrigen = valores[0];
            if (ciudadOrigen.StartsWith("Cuadro de") || ciudadOrigen.StartsWith("http")) continue;

            if (!ciudades.ContainsKey(ciudadOrigen))
            {
                ciudades[ciudadOrigen] = new Ciudad { Nombre = ciudadOrigen };
            }
            for (int j = 1; j < Math.Min(encabezados.Count, valores.Count); j++)
            {
                string ciudadDestino = encabezados[j];
                if (string.IsNullOrEmpty(ciudadDestino)) continue;

                if (double.TryParse(valores[j], out double distancia))
                {
                    ciudades[ciudadOrigen].Distancias[ciudadDestino] = distancia;
                }
            }
        }
        return ciudades;
    }

    private static List<(string Ciudad, double Promedio)> CalcularDistanciasPromedio (Dictionary<string, Ciudad> ciudades, List<string> ciudadesSeleccionadas)
    {
        var resultados = new List<(string, double)>();
        var nombresNormalizados = ciudades.ToDictionary(
            kv => NormalizarTexto(kv.Key),
            kv => kv.Key
        );

        foreach (var ciudadInput in ciudadesSeleccionadas)
        {
            string nombreNormalizado = NormalizarTexto(ciudadInput);

            if (!nombresNormalizados.TryGetValue(nombreNormalizado, out string nombreReal))
            {
                Console.WriteLine($"La ciudad '{ciudadInput}' no se encuentra en los datos del archivo");
                continue;
            }
            var ciudad = ciudades[nombreReal];
            var distancias = new List<double>();

            foreach (var otraCiudadInput in ciudadesSeleccionadas)
            {
                string NombreNormalizado2 = NormalizarTexto(otraCiudadInput);

                if (!nombresNormalizados.TryGetValue(NombreNormalizado2, out string NombreReal2))
                    continue;
                if (nombreReal.Equals(NombreReal2, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ciudad.Distancias.TryGetValue(NombreReal2, out double distancia))
                {
                    distancias.Add(distancia);
                }
                else if (ciudades[NombreReal2].Distancias.TryGetValue(nombreReal, out distancia))
                {
                    distancias.Add(distancia);
                }
                else
                {
                    Console.WriteLine($"No se encontró distancia entre {nombreReal} y {NombreReal2}");
                }
            }

            if (distancias.Any())
            {
                double promedio = distancias.Average();
                resultados.Add((nombreReal, promedio));
            }
            else
            {
                Console.WriteLine($"No se encontraron distancias válidas para {nombreReal}");
            }
        }
        return resultados;
    }
}