using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;

namespace ProyectoRutasOptimas
{

    public class Punto
    {
        public int id { get; set; }
        public double coordX { get; set; }
        public double coordY { get; set; }
        public double coordZ { get; set; }
        public List<int> IdsPuntosMasCercanos { get; set; }
    }

    class Puntos
    {
        public List<Punto> ConvertirDataTableAPuntos(DataTable tabla)
        {
            // Convertir los datos del DataTable a una lista de puntos
            List<Punto> puntos = new List<Punto>();
            foreach (DataRow fila in tabla.Rows)
            {
                Punto punto = new Punto
                {
                    id = Convert.ToInt32(fila["ID_OK"]),
                    coordX = Convert.ToDouble(fila["COORD_X"]),
                    coordY = Convert.ToDouble(fila["COORD_Y"]),
                    coordZ = Convert.ToDouble(fila["COORD_Z"])
                };
                puntos.Add(punto);
            }
            return puntos;
        }



        public double[,] CrearMatrizAdyacencia(List<Punto> puntos, Dictionary<int, List<int>> vecinosMasCercanos, int cantAcopios)
        {
            int totalPuntos = puntos.Count;
            double[,] matrizAdyacencia = new double[totalPuntos, totalPuntos];

            // Inicializar la matriz con distancias 0 para los nodos que no son vecinos
            for (int i = 0; i < totalPuntos; i++)
            {
                for (int j = 0; j < totalPuntos; j++)
                {
                    matrizAdyacencia[i, j] = 0;
                }
            }

            // Asociar los últimos X puntos (destinos) con los 8 puntos más cercanos
            for (int i = totalPuntos - cantAcopios; i < totalPuntos; i++)
            {
                Punto destino = puntos[i];
                List<int> puntosMasCercanos = destino.IdsPuntosMasCercanos;

                foreach (int puntoMasCercano in puntosMasCercanos)
                {
                    matrizAdyacencia[destino.id - 1, puntoMasCercano - 1] = CalcularDistancia(destino, puntos[puntoMasCercano - 1]);
                    matrizAdyacencia[puntoMasCercano - 1, destino.id - 1] = CalcularDistancia(destino, puntos[puntoMasCercano - 1]);
                }
            }

            // Actualizar la matriz con las distancias a los vecinos más cercanos
            foreach (var par in vecinosMasCercanos)
            {
                int puntoActual = par.Key;

                foreach (int vecino in par.Value)
                {
                    double distancia = CalcularDistancia(puntos[puntoActual - 1], puntos[vecino - 1]);
                    matrizAdyacencia[puntoActual - 1, vecino - 1] = distancia;
                    matrizAdyacencia[vecino - 1, puntoActual - 1] = distancia;
                }
            }

            return matrizAdyacencia;
        }


        public Dictionary<int, List<int>> EncontrarVecinosMasCercanos(List<Punto> puntos, int numeroVecinos)
        {
            // Crear un diccionario para almacenar los vecinos más cercanos para cada punto
            Dictionary<int, List<int>> vecinosMasCercanos = new Dictionary<int, List<int>>();

            // Calcular la distancia entre cada par de puntos y encontrar los 8 vecinos más cercanos
            foreach (Punto puntoActual in puntos)
            {
                List<int> vecinos = new List<int>();

                foreach (Punto otroPunto in puntos)
                {
                    if (puntoActual.id != otroPunto.id)
                    {
                        double distancia = CalcularDistancia(puntoActual, otroPunto);
                        if (vecinos.Count < numeroVecinos)
                        {
                            vecinos.Add(otroPunto.id);
                        }
                        else
                        {
                            // Reemplazar el vecino más lejano si la distancia es menor
                            int indiceMaxDistancia = EncontrarIndiceMaxDistancia(puntoActual, vecinos, puntos);
                            double distanciaMaxima = CalcularDistancia(puntoActual, puntos[indiceMaxDistancia]);

                            if (distancia < distanciaMaxima)
                            {
                                vecinos[indiceMaxDistancia] = otroPunto.id;
                            }
                        }
                    }
                }

                vecinosMasCercanos[puntoActual.id] = vecinos;
            }

            return vecinosMasCercanos;
        }

        static int EncontrarIndiceMaxDistancia(Punto punto, List<int> vecinos, List<Punto> puntos)
        {
            // Encontrar el índice del vecino con la mayor distancia al punto actual
            int indiceMaxDistancia = 0;
            double distanciaMaxima = CalcularDistancia(punto, puntos[vecinos[0]]);

            for (int i = 1; i < vecinos.Count; i++)
            {
                double distancia = CalcularDistancia(punto, puntos[vecinos[i]]);
                if (distancia > distanciaMaxima)
                {
                    distanciaMaxima = distancia;
                    indiceMaxDistancia = i;
                }
            }

            return indiceMaxDistancia;
        }

        //public List<Punto> AsociarPuntosAdicionales(List<Punto> puntos)
        //{
        //    // Para cada punto adicional, asociarlo con el punto más cercano de la matriz de adyacencia original
        //    for (int i = puntos.Count - 3; i < puntos.Count; i++) // Últimos 3 puntos son los destinos
        //    {
        //        int puntoAdicional = i + 1;
        //        int puntoMasCercano = EncontrarPuntoMasCercano(puntos, puntoAdicional);
        //        puntos[i].IdPuntoMasCercano = puntoMasCercano;
        //    }

        //    return puntos;
        //}

        //static int EncontrarPuntoMasCercano(List<Punto> puntos, int puntoAdicional)
        //{
        //    double distanciaMinima = double.MaxValue;
        //    int puntoMasCercano = -1;

        //    foreach (var puntoOriginal in puntos.Take(puntos.Count - 3)) // Excluir los destinos
        //    {
        //        double distancia = CalcularDistancia(puntos[puntoOriginal.id - 1], puntos[puntoAdicional - 1]);
        //        if (distancia < distanciaMinima)
        //        {
        //            distanciaMinima = distancia;
        //            puntoMasCercano = puntoOriginal.id;
        //        }
        //    }

        //    return puntoMasCercano;
        //}

        public List<Punto> AsociarPuntosAdicionales(List<Punto> puntos, int cantAcopios)
        {
            int totalPuntos = puntos.Count;

            // Para cada punto adicional (destino), asociarlo con los 8 puntos originales más cercanos (sin incluir destinos)
            for (int i = totalPuntos - cantAcopios; i < totalPuntos; i++) // Últimos X puntos son los destinos
            {
                int puntoDestino = i + 1;

                // Encontrar los 8 puntos originales más cercanos (sin incluir destinos)
                List<int> puntosMasCercanos = EncontrarPuntosMasCercanos(puntos.Take(totalPuntos - cantAcopios).ToList(), puntos, puntoDestino, 8);

                puntos[i].IdsPuntosMasCercanos = puntosMasCercanos;
            }

            return puntos;
        }


        static List<int> EncontrarPuntosMasCercanos(List<Punto> puntosOriginales, List<Punto> puntos, int puntoDestino, int cantidad)
        {
            // Ordenar los puntos originales por distancia al punto destino
            List<int> puntosMasCercanos = puntosOriginales
                .OrderBy(puntoOriginal => CalcularDistancia(puntoOriginal, puntos[puntoDestino - 1]))
                .Select(puntoOriginal => puntoOriginal.id)
                .Take(cantidad)
                .ToList();

            return puntosMasCercanos;
        }



        static double CalcularDistancia(Punto punto1, Punto punto2)
        {
            // Calcular la distancia euclidiana entre dos puntos en un espacio tridimensional
            double dx = punto1.coordX - punto2.coordX;
            double dy = punto1.coordY - punto2.coordY;
            double dz = punto1.coordZ - punto2.coordZ;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

    }
}
