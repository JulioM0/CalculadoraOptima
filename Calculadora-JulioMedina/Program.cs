using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public class CiudadOptimaCalculator
{
    public class Ciudad
    {
        public string Nombre { get; set; }
        public Dictionary<string, double> Distancias { get; set; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    }
    private static string rutaDelArchivo = "C:\\Users\\julio\\OneDrive\\Documentos\\mx.csv";
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("CALCULADORA DE CIUDAD ÓPTIMA");
            Console.WriteLine("=========================================");
            Console.WriteLine("\nMenú Principal:");
            Console.WriteLine("1. Calcular ciudad óptima");
            Console.WriteLine("2. Salir");

            switch (Console.ReadLine())
            {
                case "1":
                    Calcular(rutaDelArchivo);
                    break;
                case "2":
                    return;
                default:
                    Console.WriteLine("\nOpción no válida. Intente nuevamente.");
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
            Console.WriteLine($"Error: No se encontró el archivo {archivo}");
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
            Console.WriteLine($"\nCIUDAD ÓPTIMA: {ciudadOptima.Ciudad} con distancia promedio de {ciudadOptima.Promedio:F2} km");
        }
        else
        {
            Console.WriteLine("\nNo se encontraron resultados válidos.");
        }
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return texto;

        string normalizedString = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
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
        var lines = File.ReadAllLines(filePath);

        if (lines.Length == 0) return ciudades;

        var encabezados = lines[0].Split(',').Select(x => x.Trim('"')).ToList();

        for (int i = 1; i < lines.Length; i++)
        {
            var valores = lines[i].Split(',').Select(x => x.Trim('"')).ToList();
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
                Console.WriteLine($"Advertencia: Ciudad '{ciudadInput}' no encontrada en los datos.");
                continue;
            }

            var ciudad = ciudades[nombreReal];
            var distancias = new List<double>();

            foreach (var otraCiudadInput in ciudadesSeleccionadas)
            {
                string otraNombreNormalizado = NormalizarTexto(otraCiudadInput);

                if (!nombresNormalizados.TryGetValue(otraNombreNormalizado, out string otraNombreReal))
                    continue;

                if (nombreReal.Equals(otraNombreReal, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ciudad.Distancias.TryGetValue(otraNombreReal, out double distancia))
                {
                    distancias.Add(distancia);
                }
                else if (ciudades[otraNombreReal].Distancias.TryGetValue(nombreReal, out distancia))
                {
                    distancias.Add(distancia);
                }
                else
                {
                    Console.WriteLine($"Advertencia: No se encontró distancia entre {nombreReal} y {otraNombreReal}");
                }
            }

            if (distancias.Any())
            {
                double promedio = distancias.Average();
                resultados.Add((nombreReal, promedio));
            }
            else
            {
                Console.WriteLine($"Advertencia: No se encontraron distancias válidas para {nombreReal}");
            }
        }

        return resultados;
    }
}