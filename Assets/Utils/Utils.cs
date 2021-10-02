using UnityEngine;

namespace Utils
{
    public class Utils
    {
        public static void Print2DArray<T>(T[,] matrix)
        {
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                var line = "";
                for (var j = 0; j < matrix.GetLength(1); j++) line += matrix[i, j] + "\t";
                Debug.Log(line);
            }
        }
    }
}