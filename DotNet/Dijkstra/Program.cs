using System;

namespace Dijkstra
{
  class Program
  {
    static void Main(string[] args)
    {
      int[,] graph = new int[,] {
        { 0, 7, 0, 4, 5 },
        { 7, 0, 0, 6, 0 },
        { 0, 0, 0, 0, 0 },
        { 4, 6, 0, 0, 5 },
        { 5, 0, 0, 5, 0 }
      };

      int vertexCount = Convert.ToInt32(Math.Sqrt(graph.Length));

      for (int i = 0; i < vertexCount; i++)
      {
        for (int j = 0; j < vertexCount; j++)
        {
          Console.Write((graph[i, j].ToString() + ",").PadRight(3));
        }
        Console.WriteLine();
      }
      Console.WriteLine();


      int source = 2;


      bool[] setVertices = new bool[vertexCount];

      int[] distances = new int[vertexCount];
      for (int i = 0; i < vertexCount; i++)
      {
        distances[i] = Int32.MaxValue;
      }




      distances[source] = 0;
      for (int count = 0; count < vertexCount - 1; count++)
      {
        int curr = NextCurrentVertex(setVertices, distances, vertexCount);
        Console.WriteLine(curr + " " + distances[curr]);

        setVertices[curr] = true;

        for (int v = 0; v < vertexCount; v++)
        {
          if (!setVertices[v] && // didn't finish checking this vertice
              graph[curr, v] != 0 && // v and curr vertices are connected
              distances[curr] < Int32.MaxValue && // curr vertice is connected to source
              distances[curr] + graph[curr, v] < distances[v])
          {
            distances[v] = distances[curr] + graph[curr, v];
          }
        }
      }



      Console.WriteLine("Distances: ");
      for (int v = 0; v < vertexCount; v++)
      {
        Console.WriteLine(v + ": " + distances[v]);
      }

      Console.ReadLine();
    }

    private static int NextCurrentVertex(bool[] setVertices, int[] distances, int vertexCount)
    {
      int minVertex = -1;
      int minDist = Int32.MaxValue;

      for (int v = 0; v < vertexCount; v++)
      {
        if (!setVertices[v] && distances[v] <= minDist)
        {
          minVertex = v;
          minDist = distances[v];
        }
      }

      return minVertex;
    }
  }
}
