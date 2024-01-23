using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Diagnostics;

namespace ProyectoRutasOptimas
{
    class Program
    {
        static void Main(string[] args)
        {
            // Esperar a que el usuario presione una tecla para iniciar el proceso
            Console.WriteLine("Presione cualquier tecla para iniciar el proceso...");
            Console.ReadKey();

            // Crear un objeto Stopwatch
            Stopwatch stopwatch = new Stopwatch();

            // Iniciar el cronómetro
            stopwatch.Start();

            try
            {
                
                // Obtener la ruta completa del archivo conf.txt en la misma carpeta que la aplicación
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "conf.txt");

                // Leer los registros del archivo y crea el diccionado
                Dictionary<string, string> registros = LeerRegistrosDesdeArchivo(filePath);

                //Inicia clase puntos
                Puntos cPuntos = new Puntos();

                int numeroCiclo = 0;

                List<Punto> puntos = null;

                List<Punto> puntosAcopios = null;

                Dictionary<int, List<int>> vecinosMasCercanos = null;

                int siguienteId = 0;

                bool iniciarProcesoRutas = false;

                double[,] matrizAdyacencia = null;

                int cantidadAcopios = 0;

                int numeroVecinos = 8;

                foreach (var registro in registros)
                {

                    Console.WriteLine($"Valor de {registro.Key}: {registro.Value}");

                    // Crear un DataTable con el nombre asociado de registro.Key
                    DataTable tabla = LeerArchivoCSV(registro.Value);

                    switch (numeroCiclo)
                    {
                        case 0:
                            // Convertir los datos del DataTable a una lista de puntos
                            puntos = cPuntos.ConvertirDataTableAPuntos(tabla);

                            // Asociar cada punto con sus 8 vecinos más cercanos
                            vecinosMasCercanos = cPuntos.EncontrarVecinosMasCercanos(puntos, numeroVecinos);

                            ////IMPRIME VECINOS
                            ///// Crear el nombre del archivo CSV
                            string nombreArchivo = "Vecinos" + numeroVecinos + registro.Key + ".csv";

                            // Escribir las conexiones en el archivo CSV
                            EscribirConexionesCSV(nombreArchivo, puntos, vecinosMasCercanos);

                            break;
                        case 1:
                            // Obtener el siguiente ID correlativo para los puntos adicionales
                            siguienteId = puntos.Count + 1;

                            // Convertir los datos del DataTable a una lista de puntos
                            puntosAcopios = cPuntos.ConvertirDataTableAPuntos(tabla);

                            cantidadAcopios = puntosAcopios.Count;


                            // Agregar puntos acopios como destinos
                            foreach (var puntoAcopio in puntosAcopios)
                            {
                                puntos.Add(new Punto { id = siguienteId++, coordX = puntoAcopio.coordX, coordY = puntoAcopio.coordY, coordZ = puntoAcopio.coordZ });
                            }

                            // Asociar cada punto adicional con el punto más cercano de la matriz de adyacencia original
                            puntos = cPuntos.AsociarPuntosAdicionales(puntos, cantidadAcopios);

                            ////IMPRIME VECINOS
                            ///// Crear el nombre del archivo CSV
                            nombreArchivo = "Vecinos" + numeroVecinos + registro.Key + ".csv";

                            // Escribir las conexiones en el archivo CSV
                            EscribirGrafoCSV(nombreArchivo, puntos);

                            // Crear la matriz de adyacencia con los puntos adicionales al final
                            matrizAdyacencia = cPuntos.CrearMatrizAdyacencia(puntos, vecinosMasCercanos, cantidadAcopios);

                            iniciarProcesoRutas = true;
                            break;
                    }
                    //Se aumenta numero de ciclo para pasar al siguiente archivo
                    numeroCiclo = numeroCiclo == 1 ? 0 : numeroCiclo + 1;

                    //INICIO PROCESO RUTAS MAS CORTAS
                    if (iniciarProcesoRutas)
                    {

                        procesoDijkstra(puntos, matrizAdyacencia, cantidadAcopios);
                        Console.WriteLine("Resultados finales Dijkstra:");

                        EscribirCSV("SalidaDijkstra"+ registro.Key + ".csv", distanciasMinimas, nodosInicioOptimos, caminosOptimos);

                        //IMPRIMIR CONEXIONES
                        // Procesa el archivo de salida Dijkstra y genera el nuevo CSV
                        GenerarCSVCoordenadasSegmentos("SalidaDijkstra" + registro.Key + ".csv", "SegmentosSalida" + registro.Key + ".csv", puntos);


                        procesoAStar(puntos, matrizAdyacencia, cantidadAcopios);
                        Console.WriteLine("Resultados finales A Star:");

                        EscribirCSV("SalidaAStar" + registro.Key + ".csv", distanciasMinimas, nodosInicioOptimos, caminosOptimos);
                        //IMPRIMIR CONEXIONES
                        GenerarCSVCoordenadasSegmentos("SalidaAStar" + registro.Key + ".csv", "SegmentosSalidaAStar" + registro.Key + ".csv", puntos);


                        procesoBellmanFord(puntos, matrizAdyacencia, cantidadAcopios);
                        Console.WriteLine("Resultados finales BellmanFord:");

                        EscribirCSV("SalidaBellmanFord" + registro.Key + ".csv", distanciasMinimas, nodosInicioOptimos, caminosOptimos);
                        //IMPRIMIR CONEXIONES
                        GenerarCSVCoordenadasSegmentos("SalidaBellmanFord" + registro.Key + ".csv", "SegmentosSalidaBellmanFord" + registro.Key + ".csv", puntos);

                        //SE DESACTIVA PROCESO PARA RUTAS OPTIMAS
                        iniciarProcesoRutas = false;



                        //Generacion archivo consolidado
                        // Rutas de los archivos CSV
                        string[] rutasArchivos = { "SalidaDijkstra" + registro.Key + ".csv", "SalidaAStar" + registro.Key + ".csv", "SalidaBellmanFord" + registro.Key + ".csv" };

                        // Leer los datos de cada archivo
                        var datosArchivos = rutasArchivos.Select(LeerArchivo).ToList();

                        // Consolidar los datos
                        var consolidado = ConsolidarDatos(datosArchivos);

                        // Escribir el archivo consolidado
                        EscribirArchivoConsolidado("ConsolidadoAlgoritmos" + registro.Key + ".csv", consolidado);

                        Console.WriteLine("Generacion archivo consolidado");

                    }


                }


                // Detener el cronómetro
                stopwatch.Stop();

                // Obtener el tiempo transcurrido en milisegundos
                long tiempo = stopwatch.ElapsedMilliseconds;

                // Imprimir el tiempo transcurrido
                Console.WriteLine("Tiempo total de ejecución: {0} milisegundos.", tiempo);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }


            // Esperar a que el usuario presione una tecla antes de salir
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }



        //EJEMPLO VECINOS
        static void EscribirConexionesCSV(string nombreArchivo, List<Punto> puntos, Dictionary<int, List<int>> vecinos)
        {
            using (var writer = new StreamWriter(nombreArchivo))
            {
                // Escribir encabezados
                writer.WriteLine("ID;CoordXInicio;CoordYInicio;CoordZInicio;CoordXFin;CoordYFin;CoordZFin");
                int numeroID = 0;
                // Escribir conexiones
                foreach (var punto in puntos)
                {
                    if (vecinos.ContainsKey(punto.id))
                    {
                        foreach (var vecinoId in vecinos[punto.id])
                        {
                            var vecino = puntos.Find(v => v.id == vecinoId);
                            writer.WriteLine($"{numeroID};{punto.coordX};{punto.coordY};{punto.coordZ};{vecino.coordX};{vecino.coordY};{vecino.coordZ}");
                            numeroID++;
                        }
                    }
                }
            }
        }

        static void EscribirGrafoCSV(string nombreArchivo, List<Punto> puntos)
        {
            using (var writer = new StreamWriter(nombreArchivo))
            {
                // Escribir encabezados
                writer.WriteLine("ID;CoordXInicio;CoordYInicio;CoordZInicio;CoordXFin;CoordYFin;CoordZFin");
                int numeroID = 0;

                Dictionary<int, List<int>> vecinos = puntos.Where(p => p.IdsPuntosMasCercanos != null)
                                           .ToDictionary(p => p.id, p => p.IdsPuntosMasCercanos);
                // Escribir conexiones
                foreach (var punto in puntos)
                {
                    if (vecinos.ContainsKey(punto.id))
                    {
                        foreach (var vecinoId in vecinos[punto.id])
                        {
                            var vecino = puntos.Find(v => v.id == vecinoId);
                            writer.WriteLine($"{numeroID};{punto.coordX};{punto.coordY};{punto.coordZ};{vecino.coordX};{vecino.coordY};{vecino.coordZ}");
                            numeroID++;
                        }
                    }
                }
            }
        }


        //EJEMPLO CAMINOS
        static void GenerarCSVCoordenadasSegmentos(string rutaArchivoOptimo, string rutaNuevoCSV, List<Punto> puntos)
        {
            // Lee las líneas del archivo de salida Dijkstra
            var lineas = File.ReadAllLines(rutaArchivoOptimo);

            // Abre el archivo para escritura
            using (var writer = new StreamWriter(rutaNuevoCSV))
            {
                // Escribe el encabezado
                writer.WriteLine("ID;CoordXInicio;CoordYInicio;CoordZInicio;CoordXFin;CoordYFin;CoordZFin");
                int idCamino = 0;
                // Itera sobre las líneas del archivo de salida Dijkstra
                foreach (var linea in lineas.Skip(1)) // Salta el encabezado
                {
                    // Divide la línea por el separador (;)
                    var elementos = linea.Split(';');

                    // Obtiene el camino óptimo
                    var caminoOptimo = elementos[3].Split("->").Select(int.Parse).ToList();

                    // Itera sobre el camino óptimo para crear segmentos de línea
                    for (int i = 0; i < caminoOptimo.Count - 1; i++)
                    {
                        var puntoInicio = puntos.First(p => p.id == caminoOptimo[i] + 1);
                        var puntoFin = puntos.First(p => p.id == caminoOptimo[i + 1] + 1);
                        writer.WriteLine($"{idCamino};{puntoInicio.coordX};{puntoInicio.coordY};{puntoInicio.coordZ};{puntoFin.coordX};{puntoFin.coordY};{puntoFin.coordZ}");
                        idCamino++;
                    }
                }
            }
        }


        static Dictionary<string, string> LeerRegistrosDesdeArchivo(string filePath)
        {
            Dictionary<string, string> registros = new Dictionary<string, string>();

            try
            {
                // Verificar si el archivo existe
                if (File.Exists(filePath))
                {
                    // Leer todas las líneas del archivo
                    string[] lines = File.ReadAllLines(filePath);

                    // Iterar sobre cada línea y extraer los valores de los registros
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');

                        if (parts.Length == 2)
                        {
                            string key = parts[0];
                            string value = parts[1];

                            registros[key] = value;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("El archivo " + filePath + " no se encontró en la misma carpeta que la aplicación.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo: {ex.Message}");
            }

            return registros;
        }

        static DataTable LeerArchivoCSV(string nombreArchivo)
        {

            DataTable tabla = new DataTable();
            try
            {
                // Obtener la ruta completa del archivo CSV en la misma carpeta que la aplicación
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);

                // Verificar si el archivo existe
                if (File.Exists(filePath))
                {
                    // Utilizar StreamReader para leer el archivo línea por línea
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        // Leer la primera línea para obtener los nombres de las columnas
                        string[] columnas = reader.ReadLine().Split(';');

                        // Agregar las columnas a la tabla
                        foreach (string columna in columnas)
                        {
                            tabla.Columns.Add(columna.Trim()); // Puedes ajustar el trim según sea necesario
                        }

                        // Leer el resto de las líneas y agregarlas como filas a la tabla
                        while (!reader.EndOfStream)
                        {
                            string[] valores = reader.ReadLine().Split(';');

                            // Evitar agregar filas vacías al final del archivo
                            if (!filaVacia(valores))
                            {

                                DataRow fila = tabla.NewRow();

                                for (int i = 0; i < columnas.Length; i++)
                                {
                                    fila[i] = valores[i].Trim(); // Puedes ajustar el trim según sea necesario
                                }

                                tabla.Rows.Add(fila);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"El archivo {nombreArchivo} no se encontró en la misma carpeta que la aplicación.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo: {ex.Message}");
            }

            return tabla;
        }


        static bool filaVacia(string[] valores)
        {
            foreach (var valor in valores)
            {
                if (!string.IsNullOrWhiteSpace(valor))
                {
                    return false;
                }
            }
            return true;
        }


        // Inicializar estructuras para seguimiento de distancias
        static Dictionary<int, double> distanciasMinimas;
        static Dictionary<int, int> nodosInicioOptimos;
        static Dictionary<int, List<int>> caminosOptimos;


        static void procesoDijkstra(List<Punto> puntos, double[,] matrizAdyacencia, int cantAcopios)
        {
            // Crear una instancia de la clase Dijkstra
            Dijkstra dijkstra = new Dijkstra();

            // Definir los nodos de inicio (todos los nodos acopios en este caso) y los destinos
            List<int> nodosInicio = new List<int>();
            for (int i = puntos.Count - cantAcopios; i < puntos.Count; i++)
            {
                nodosInicio.Add(i);
            }
            List<int> destinos = Enumerable.Range(0, puntos.Count - cantAcopios).ToList(); // Todos los nodos excepto los últimos X

            // Inicializar estructuras para seguimiento de distancias
            distanciasMinimas = new Dictionary<int, double>();
            nodosInicioOptimos = new Dictionary<int, int>();
            caminosOptimos = new Dictionary<int, List<int>>();

            // Calcular las distancias y caminos más cortos desde cada nodo de inicio a los destinos
            foreach (int nodoInicio in nodosInicio)
            {
                var resultadoDijkstra = dijkstra.CalcularDistancias(matrizAdyacencia, nodoInicio, destinos);

                // Obtener las distancias más cortas y los caminos desde el nodo de inicio a los destinos
                double[] distancias = resultadoDijkstra.Item1;
                Dictionary<int, List<int>> caminos = resultadoDijkstra.Item2;

                // Actualizar estructuras de seguimiento
                foreach (int destino in destinos)
                {
                    double distanciaActual = distancias[destino];

                    if (!distanciasMinimas.ContainsKey(destino) || distanciaActual < distanciasMinimas[destino])
                    {
                        distanciasMinimas[destino] = distanciaActual;
                        nodosInicioOptimos[destino] = nodoInicio;
                        if (caminos.ContainsKey(destino))
                        {
                            caminosOptimos[destino] = new List<int>(caminos[destino].Prepend(nodoInicio));
                        }
                        else
                        {
                            // Si no hay camino calculado para el destino, puedes manejarlo de acuerdo a tus necesidades
                            caminosOptimos[destino] = new List<int> { nodoInicio };
                        }

                    }
                }
            }
        }

        static void procesoAStar(List<Punto> puntos, double[,] matrizAdyacencia, int cantAcopios)
        {
            //A Star
            // Crear una instancia de la clase AStar
            AStar aStar = new AStar();

            // Definir los nodos de inicio (todos los nodos acopios en este caso) y los destinos
            List<int> nodosInicio = new List<int>();
            for (int i = puntos.Count - cantAcopios; i < puntos.Count; i++)
            {
                nodosInicio.Add(i); // Sumar 1 para ajustar a los índices de 1 a N
            }
            List<int> destinos = Enumerable.Range(0, puntos.Count - cantAcopios).ToList(); // Todos los nodos excepto los últimos X

            // Inicializar estructuras para seguimiento de distancias
            distanciasMinimas = new Dictionary<int, double>();
            nodosInicioOptimos = new Dictionary<int, int>();
            caminosOptimos = new Dictionary<int, List<int>>();


            // Calcular las distancias y caminos más cortos desde cada nodo de inicio a los destinos
            foreach (int nodoInicio in nodosInicio)
            {
                var resultadoAStar = aStar.CalcularDistanciasAStar(matrizAdyacencia, nodoInicio, destinos, (actual, destino) => aStar.CalcularHeuristica(actual, destino - 1, puntos));

                // Obtener las distancias más cortas y los caminos desde el nodo de inicio a los destinos
                double[] distancias = resultadoAStar.Item1;
                Dictionary<int, List<int>> caminos = resultadoAStar.Item2;

                // Actualizar estructuras de seguimiento
                foreach (int destino in destinos)
                {
                    double distanciaActual = distancias[destino];

                    if (!distanciasMinimas.ContainsKey(destino) || distanciaActual < distanciasMinimas[destino])
                    {
                        distanciasMinimas[destino] = distanciaActual;
                        nodosInicioOptimos[destino] = nodoInicio;
                        if (caminos.ContainsKey(destino))
                        {
                            caminosOptimos[destino] = new List<int>(caminos[destino].Prepend(nodoInicio));
                        }
                        else
                        {
                            // Si no hay camino calculado para el destino, puedes manejarlo de acuerdo a tus necesidades
                            caminosOptimos[destino] = new List<int> { nodoInicio };
                        }

                    }
                }
            }

        }


        static void procesoBellmanFord(List<Punto> puntos, double[,] matrizAdyacencia, int cantAcopios)
        {
            //Bellman Ford
            // Crear una instancia de la clase BellmanFord
            BellmanFord bellmanFord = new BellmanFord();

            // Definir los nodos de inicio (todos los nodos acopios en este caso) y los destinos
            List<int> nodosInicio = new List<int>();
            for (int i = puntos.Count - cantAcopios; i < puntos.Count; i++)
            {
                nodosInicio.Add(i); // Sumar 1 para ajustar a los índices de 1 a N
            }
            List<int> destinos = Enumerable.Range(0, puntos.Count - cantAcopios).ToList(); // Todos los nodos excepto los últimos X

            // Inicializar estructuras para seguimiento de distancias
            distanciasMinimas = new Dictionary<int, double>();
            nodosInicioOptimos = new Dictionary<int, int>();
            caminosOptimos = new Dictionary<int, List<int>>();


            // Calcular las distancias y caminos más cortos desde cada nodo de inicio a los destinos
            foreach (int nodoInicio in nodosInicio)
            {
                var resultadoBellmanFord = bellmanFord.CalcularDistancias(matrizAdyacencia, nodoInicio, destinos);

                // Obtener las distancias más cortas y los caminos desde el nodo de inicio a los destinos
                double[] distancias = resultadoBellmanFord.Item1;
                Dictionary<int, List<int>> caminos = resultadoBellmanFord.Item2;

                // Actualizar estructuras de seguimiento
                foreach (int destino in destinos)
                {
                    double distanciaActual = distancias[destino];

                    if (!distanciasMinimas.ContainsKey(destino) || distanciaActual < distanciasMinimas[destino])
                    {
                        distanciasMinimas[destino] = distanciaActual;
                        nodosInicioOptimos[destino] = nodoInicio;
                        if (caminos.ContainsKey(destino))
                        {
                            caminosOptimos[destino] = new List<int>(caminos[destino].Prepend(nodoInicio));
                        }
                        else
                        {
                            // Si no hay camino calculado para el destino
                            caminosOptimos[destino] = new List<int> { nodoInicio };
                        }

                    }
                }
            }

        }


        static void EscribirCSV(string rutaArchivoCSV, Dictionary<int, double> distanciasMinimasSalida,
                           Dictionary<int, int> nodosInicioOptimosSalida, Dictionary<int, List<int>> caminosOptimosSalida)
        {
            using (StreamWriter sw = new StreamWriter(rutaArchivoCSV))
            {
                // Escribir encabezados
                sw.WriteLine("Destino;DistanciaMinima;NodoInicioOptimo;CaminoOptimo");

                foreach (int destino in distanciasMinimasSalida.Keys)
                {
                    // Obtener valores correspondientes a cada destino
                    double distanciaMinima = distanciasMinimasSalida.ContainsKey(destino) ? distanciasMinimasSalida[destino] : double.MaxValue;
                    int nodoInicioOptimo = nodosInicioOptimosSalida.ContainsKey(destino) ? nodosInicioOptimosSalida[destino] : -1;
                    List<int> caminoOptimo = caminosOptimosSalida.ContainsKey(destino) ? caminosOptimosSalida[destino] : new List<int>();

                    // Escribir una línea en el archivo CSV
                    sw.WriteLine($"{destino};{distanciaMinima};{nodoInicioOptimo};{string.Join("->", caminoOptimo)}");
                }
            }

            Console.WriteLine($"Resultados guardados en: {rutaArchivoCSV}");
        }



        //Funciones para archivo consolidado
        static List<string[]> LeerArchivo(string nombreArchivo)
        {
            var datos = new List<string[]>();

            using (var reader = new StreamReader(nombreArchivo))
            {
                // Leer encabezados
                var encabezados = reader.ReadLine().Split(';');

                while (!reader.EndOfStream)
                {
                    // Leer línea de datos
                    var linea = reader.ReadLine().Split(';');
                    datos.Add(linea);
                }
            }

            return datos;
        }

        static List<string[]> ConsolidarDatos(List<List<string[]>> datosArchivos)
        {
            var consolidado = new List<string[]>();
            var destinos = datosArchivos.First().Select(linea => linea[0]).ToList();

            foreach (var destino in destinos)
            {
                var registroConsolidado = new List<string> { destino };

                for (int i = 0; i < datosArchivos.Count; i++)
                {
                    var datosArchivo = datosArchivos[i].FirstOrDefault(linea => linea[0] == destino);

                    if (datosArchivo != null)
                    {
                        registroConsolidado.Add(datosArchivo[1]); // Distancia mínima
                        registroConsolidado.Add(datosArchivo[2]); // Nodo inicio óptimo
                        registroConsolidado.Add(datosArchivo[3]); // Camino óptimo
                    }
                    else
                    {
                        // Puedes manejar el caso en que un destino no esté presente en uno de los archivos
                        registroConsolidado.AddRange(Enumerable.Repeat("", 3));
                    }
                }

                consolidado.Add(registroConsolidado.ToArray());
            }

            return consolidado;
        }

        static void EscribirArchivoConsolidado(string nombreArchivo, List<string[]> datos)
        {
            using (var writer = new StreamWriter(nombreArchivo))
            {
                // Escribir encabezados
                var encabezados = "ID;DistanciaMinimaDijkstra;NodoInicioOptimoDijkstra;CaminoOptimoDijkstra;DistanciaMinimaAStar;NodoInicioOptimoAStar;CaminoOptimoAStar;DistanciaMinimaBellmanFord;NodoInicioOptimoBellmanFord;CaminoOptimoBellmanFord";
                writer.WriteLine(string.Join(";", encabezados));

                // Escribir datos
                foreach (var registro in datos)
                {
                    writer.WriteLine(string.Join(";", registro));
                }
            }
        }
    













        //static List<Dictionary<int, Tuple<double, int, List<int>>>> LeerArchivosCSV(string[] rutasArchivos)
        //{
        //    List<Dictionary<int, Tuple<double, int, List<int>>>> resultados = new List<Dictionary<int, Tuple<double, int, List<int>>>>();

        //    foreach (string rutaArchivo in rutasArchivos)
        //    {
        //        Dictionary<int, Tuple<double, int, List<int>>> resultado = new Dictionary<int, Tuple<double, int, List<int>>>();

        //        using (StreamReader sr = new StreamReader(rutaArchivo))
        //        {
        //            // Ignorar la primera línea (encabezados)
        //            sr.ReadLine();

        //            while (!sr.EndOfStream)
        //            {
        //                string linea = sr.ReadLine();
        //                string[] campos = linea.Split(';');

        //                int destino = int.Parse(campos[0]);
        //                double distanciaMinima = double.Parse(campos[1]);
        //                int nodoInicioOptimo = int.Parse(campos[2]);
        //                List<int> caminoOptimo = campos[3].Split("->").Select(int.Parse).ToList();

        //                resultado[destino] = Tuple.Create(distanciaMinima, nodoInicioOptimo, caminoOptimo);
        //            }
        //        }

        //        resultados.Add(resultado);
        //    }

        //    return resultados;
        //}

        //static Dictionary<int, Tuple<double, int, List<int>>> ConsolidarResultados(List<Dictionary<int, Tuple<double, int, List<int>>>> resultados)
        //{
        //    Dictionary<int, Tuple<double, int, List<int>>> consolidado = new Dictionary<int, Tuple<double, int, List<int>>>();

        //    foreach (var resultado in resultados)
        //    {
        //        foreach (var kvp in resultado)
        //        {
        //            int destino = kvp.Key;

        //            if (!consolidado.ContainsKey(destino))
        //            {
        //                consolidado[destino] = Tuple.Create(double.NaN, 0, new List<int>());
        //            }

        //            Tuple<double, int, List<int>> valorActual = consolidado[destino];
        //            Tuple<double, int, List<int>> nuevoValor = kvp.Value;

        //            consolidado[destino] = Tuple.Create(
        //                AgregarValor(valorActual.Item1, nuevoValor.Item1),
        //                AgregarValor(valorActual.Item2, nuevoValor.Item2),
        //                AgregarValor(valorActual.Item3, nuevoValor.Item3)
        //            );
        //        }
        //    }

        //    return consolidado;
        //}

        //static T AgregarValor<T>(T valorActual, T nuevoValor)
        //{
        //    if (typeof(T) == typeof(double) && double.IsNaN(Convert.ToDouble(valorActual)))
        //    {
        //        // Si el valor actual es NaN, se reemplaza con el nuevo valor
        //        return nuevoValor;
        //    }

        //    return valorActual;
        //}


        //static void EscribirCSVConsolidado(string rutaArchivoCSV, Dictionary<int, Tuple<double, int, List<int>>> consolidado)
        //{
        //    using (StreamWriter sw = new StreamWriter(rutaArchivoCSV))
        //    {
        //        // Escribir encabezados
        //        sw.WriteLine("Destino,DistanciaDijkstra,DistanciaAStar,DistanciaBellmanFord,NodoInicioDijkstra,NodoInicioOptimoAStar,NodoInicioOptimoBellmanFord,CaminoOptimoDijkstra,CaminoOptimoAStar,CaminoOptimoBellmanFord");

        //        foreach (int destino in consolidado.Keys)
        //        {
        //            Tuple<double, int, List<int>> valores = consolidado[destino];
        //            sw.WriteLine($"{destino},{valores.Item1},{double.NaN},{double.NaN},{valores.Item2},{0},{0},{string.Join("->", valores.Item3)},{string.Empty},{string.Empty}");
        //        }
        //    }
        //}


        ////IMPRIMIR ALGORITMO
        //static void ImprimirResultados(int inicio, int fin, (int[], Dictionary<int, List<int>>) resultado)
        //{
        //    int[] distancia = resultado.Item1;
        //    Dictionary<int, List<int>> caminos = resultado.Item2;

        //    Console.WriteLine($"Camino desde el nodo {inicio} al nodo {fin}:");
        //    ImprimirCaminoYDistancia(inicio, fin, distancia, caminos[fin]);
        //}

        //static void ImprimirCaminoYDistancia(int inicio, int fin, int[] distancia, List<int> camino)
        //{
        //    Console.Write($"Camino: {inicio} ");
        //    foreach (var nodo in camino)
        //    {
        //        Console.Write($"-> {nodo} ");
        //    }
        //    Console.WriteLine($"\nDistancia total: {distancia[fin]}");
        //}
    }
}
