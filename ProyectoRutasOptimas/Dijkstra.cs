using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ProyectoRutasOptimas
{
    public class Dijkstra
    {
        public (double[], Dictionary<int, List<int>>) CalcularDistancias(double[,] grafo, int inicio, List<int> destinos)
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
                int u = MinDistancia(distancia, visitado);
                visitado[u] = true;

                for (int v = 0; v < n; v++)
                {
                    if (!visitado[v] && grafo[u, v] != 0 && distancia[u] != int.MaxValue &&
                        distancia[u] + grafo[u, v] < distancia[v])
                    {
                        distancia[v] = distancia[u] + grafo[u, v];
                        caminos[v] = new List<int>(caminos[u]) { v };
                    }
                }
            }

            // Antes de retornar, verificamos si los destinos son alcanzables
            Dictionary<int, List<int>> caminosFinales = new Dictionary<int, List<int>>();
            foreach (int destino in destinos)
            {
                if (distancia[destino] != int.MaxValue)
                {
                    caminosFinales[destino] = caminos[destino];
                }
                else
                {
                    // Si el destino no es alcanzable, devolvemos un camino vacío
                    caminosFinales[destino] = new List<int>();
                }
            }

            return (distancia, caminosFinales);
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

        private int MinDistancia(double[] distancia, bool[] visitado)
        {
            double min = double.MaxValue;
            int minIndex = -1;

            for (int v = 0; v < distancia.Length; v++)
            {
                if (!visitado[v] && distancia[v] <= min)
                {
                    min = distancia[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }

        //private Dictionary<int, List<int>> FiltrarCaminos(Dictionary<int, List<int>> caminos, List<int> destinos)
        //{
        //    var caminosFiltrados = new Dictionary<int, List<int>>();

        //    foreach (var kvp in caminos)
        //    {
        //        int nodo = kvp.Key;
        //        List<int> camino = kvp.Value;

        //        // Filtrar el camino para incluir solo los destinos especificados
        //        List<int> caminoFiltrado = camino.Where(destinos.Contains).ToList();

        //        if (caminoFiltrado.Any())
        //        {
        //            caminosFiltrados[nodo] = caminoFiltrado;
        //        }
        //    }

        //    return caminosFiltrados;
        //}
    }

}
