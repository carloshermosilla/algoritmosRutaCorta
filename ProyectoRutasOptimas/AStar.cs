using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ProyectoRutasOptimas
{
    public class AStar
    {
        public (double[], Dictionary<int, List<int>>) CalcularDistanciasAStar(double[,] grafo, int inicio, List<int> destinos, Func<int, int, double> heuristica)
        {
            int n = grafo.GetLength(0);
            double[] distancia = new double[n];
            bool[] visitado = new bool[n];
            Dictionary<int, List<int>> caminos = InicializarCaminos(n);

            for (int i = 0; i < n; i++)
            {
                distancia[i] = double.MaxValue;
                visitado[i] = false;
            }

            distancia[inicio] = 0;

            for (int count = 0; count < n - 1; count++)
            {
                int u = MinDistanciaAStar(distancia, visitado, heuristica);
                visitado[u] = true;

                for (int v = 0; v < n; v++)
                {
                    if (!visitado[v] && grafo[u, v] != 0 && distancia[u] != double.MaxValue &&
                        distancia[u] + grafo[u, v] < distancia[v])
                    {
                        distancia[v] = distancia[u] + grafo[u, v];
                        caminos[v] = new List<int>(caminos[u]) { v };
                    }
                }
            }

            Dictionary<int, List<int>> caminosFinales = new Dictionary<int, List<int>>();
            foreach (int destino in destinos)
            {
                if (distancia[destino] != double.MaxValue)
                {
                    caminosFinales[destino] = caminos[destino];
                }
                else
                {
                    caminosFinales[destino] = new List<int>();
                }
            }

            return (distancia, caminosFinales);
        }

        private int MinDistanciaAStar(double[] distancia, bool[] visitado, Func<int, int, double> heuristica)
        {
            double min = double.MaxValue;
            int minIndex = -1;

            for (int v = 0; v < distancia.Length; v++)
            {
                if (!visitado[v] && distancia[v] + heuristica(v, visitado.Length) <= min)
                {
                    min = distancia[v] + heuristica(v, visitado.Length);
                    minIndex = v;
                }
            }

            return minIndex;
        }

        private Dictionary<int, List<int>> InicializarCaminos(int n)
        {
            var caminos = new Dictionary<int, List<int>>();
            for (int i = 0; i < n; i++)
            {
                caminos[i] = new List<int>();
            }
            return caminos;
        }

        public double CalcularHeuristica(int nodoActual, int nodoDestino, List<Punto> coordenadas)
        {
            // Obtener las coordenadas del nodo actual y del nodo destino
            Punto coordenadasActual = coordenadas[nodoActual];
            Punto coordenadasDestino = coordenadas[nodoDestino];

            // Calcular la distancia Euclidiana
            double distanciaEuclidiana = Math.Sqrt(Math.Pow(coordenadasActual.coordX - coordenadasDestino.coordX, 2) +
                                                   Math.Pow(coordenadasActual.coordY - coordenadasDestino.coordY, 2) +
                                                   Math.Pow(coordenadasActual.coordZ - coordenadasDestino.coordZ, 2));

            // Puedes personalizar esta función de heurística según tu problema y necesidades
            return distanciaEuclidiana;
        }
    }
}
