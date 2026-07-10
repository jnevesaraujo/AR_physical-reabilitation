using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using App.Core;

namespace App.Core.Utils
{
    public class PerformanceLogger : MonoBehaviour
    {
        [Header("Settings")]
        public float logIntervalSeconds = 0.5f; // 2 times per second

        private string _filePath;
        private bool _isLogging = false;
        private StringBuilder _csvBuilder;

        private void Start()
        {
            string exerciseName = "Exercicio_Desconhecido";
            if (SessionContext.CurrentExercise != null)
            {
                exerciseName = SessionContext.CurrentExercise.name.Replace(" ", "_");
            }
            // File path for the CSV file
            _filePath = Path.Combine(Application.persistentDataPath, $"PerformanceLog_{exerciseName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
            
            _csvBuilder = new StringBuilder();
            _csvBuilder.AppendLine("Time(s),FPS,MemoryAllocated(MB),MemoryReserved(MB)");

            _isLogging = true;
            StartCoroutine(LogPerformanceRoutine());
            
            Debug.Log($"[PerformanceLogger] A gravar dados para: {_filePath}");
        }

        private IEnumerator LogPerformanceRoutine()
        {
            float elapsedTime = 0f;

            while (_isLogging)
            {
                yield return new WaitForSeconds(logIntervalSeconds);
                elapsedTime += logIntervalSeconds;

                // Calculate FPS
                float fps = 1f / Time.unscaledDeltaTime;

                // Calculate memory usage in MB
                float allocMem = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
                float reservedMem = Profiler.GetTotalReservedMemoryLong() / 1048576f;

                // add line to CSV
                _csvBuilder.AppendLine($"{elapsedTime.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}," +
                                       $"{fps.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}," +
                                       $"{allocMem.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                                       $"{reservedMem.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            }
        }

        private void OnDestroy()
        {
            _isLogging = false;
            File.WriteAllText(_filePath, _csvBuilder.ToString());
            Debug.Log("[PerformanceLogger] Ficheiro CSV guardado com sucesso.");
        }
    }
}