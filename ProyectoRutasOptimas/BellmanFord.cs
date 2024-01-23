using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoRutasOptimas
{
    public class BellmanFord
    {
        public (double[], Dictionary<int, List<int>>) CalcularDistancias(double[,] grafo, int inicio, List<int> destinos)
        {
            int n = grafo.GetLength(0);
            double[] distancia = new double[n];
            Dictionary<int, List<int>> caminos = InicializarCaminos(n);

            // Inicializar distancias
            for (int i = 0; i < n; i++)
            {
                distancia[i] = double.MaxValue;
            }
            distancia[inicio] = 0;

            // Relajación de las aristas repetidamente
            for (int count = 0; count < n - 1; count++)
            {
                for (int u = 0; u < n; u++)
                {
                    for (int v = 0; v < n; v++)
                    {
                        if (grafo[u, v] != 0 && distancia[u] != double.MaxValue && distancia[u] + grafo[u, v] < distancia[v])
                        {
                            distancia[v] = distancia[u] + grafo[u, v];
                            caminos[v] = new List<int>(caminos[u]) { v };
                        }
                    }
                }
            }

            // Detección de ciclo negativo
            for (int u = 0; u < n; u++)
            {
                for (int v = 0; v < n; v++)
                {
                    if (grafo[u, v] != 0 && distancia[u] != double.MaxValue && distancia[u] + grafo[u, v] < distancia[v])
                    {
                        // Se ha encontrado un ciclo negativo
                        Console.WriteLine("El grafo contiene un ciclo negativo.");
                        return (null, null);
                    }
                }
            }

            // Antes de retornar, verificamos si los destinos son alcanzables
            Dictionary<int, List<int>> caminosFinales = new Dictionary<int, List<int>>();
            foreach (int destino in destinos)
            {
                if (distancia[destino] != double.MaxValue)
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
    }

}
