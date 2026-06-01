using SualtiRoketSistemi.Models;
using System.Globalization;

namespace SualtiRoketSistemi.Services
{
    public class ImuDosyaService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImuDosyaService> _logger;

        public ImuDosyaService(IWebHostEnvironment env, ILogger<ImuDosyaService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public List<string> GetDosyaListesi()
        {
            var folder = Path.Combine(_env.WebRootPath, "imu_data");
            if (!Directory.Exists(folder)) return new List<string>();
            return Directory.GetFiles(folder, "*.txt")
                .Select(f => Path.GetFileName(f) ?? string.Empty)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .OrderByDescending(f => f)
                .ToList();
        }

        public List<ImuVeri> ParseDosya(string dosyaAdi)
        {
            var filePath = Path.Combine(_env.WebRootPath, "imu_data", Path.GetFileName(dosyaAdi));
            var result = new List<ImuVeri>();
            if (!File.Exists(filePath)) return result;

            try
            {
                var lines = File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                if (lines.Count == 0) return result;

                var delimiter = lines[0].Contains('\t') ? '\t' : ',';
                var header = lines[0].Split(delimiter).Select(h => h.Trim()).ToList();
                var map = header
                    .Select((h, i) => new { Key = Normalize(h), Index = i })
                    .GroupBy(x => x.Key)
                    .ToDictionary(g => g.Key, g => g.First().Index);

                var hasNamedHeader = map.ContainsKey("time_s") || map.ContainsKey("timestamp") || map.ContainsKey("accel_x_mps2") || map.ContainsKey("ax");
                var dataLines = hasNamedHeader ? lines.Skip(1) : lines;
                var sampleNo = 1;

                foreach (var line in dataLines)
                {
                    var parts = line.Split(delimiter).Select(p => p.Trim()).ToArray();
                    if (parts.Length < 7) continue;

                    double timeS;
                    if (TryGetDouble(parts, map, "time_s", out var tsSeconds)) timeS = tsSeconds;
                    else if (TryGetDouble(parts, map, "timestamp", out var tsRaw)) timeS = tsRaw > 20 ? tsRaw / 1000.0 : tsRaw;
                    else if (TryParse(parts.ElementAtOrDefault(0), out var t0)) timeS = t0 > 20 ? t0 / 1000.0 : t0;
                    else timeS = (sampleNo - 1) * 0.5;

                    var v = new ImuVeri
                    {
                        TimeSeconds = timeS,
                        Timestamp = (int)Math.Round(timeS * 1000.0),
                        SampleNo = GetInt(parts, map, "sample_no") ?? sampleNo,
                        Phase = GetString(parts, map, "phase"),
                        State = GetString(parts, map, "state"),
                        Ax = GetFloat(parts, map, "accel_x_mps2", "ax", fallbackIndex: hasNamedHeader ? null : 1),
                        Ay = GetFloat(parts, map, "accel_y_mps2", "ay", fallbackIndex: hasNamedHeader ? null : 2),
                        Az = GetFloat(parts, map, "accel_z_mps2", "az", fallbackIndex: hasNamedHeader ? null : 3),
                        Gx = GetFloat(parts, map, "gyro_x_dps", "gx", fallbackIndex: hasNamedHeader ? null : 4),
                        Gy = GetFloat(parts, map, "gyro_y_dps", "gy", fallbackIndex: hasNamedHeader ? null : 5),
                        Gz = GetFloat(parts, map, "gyro_z_dps", "gz", fallbackIndex: hasNamedHeader ? null : 6),
                        RollDeg = GetNullableFloat(parts, map, "roll_deg"),
                        PitchDeg = GetNullableFloat(parts, map, "pitch_deg"),
                        YawDeg = GetNullableFloat(parts, map, "yaw_deg"),
                        DepthM = GetNullableFloat(parts, map, "depth_m"),
                        DistanceFromStartM = GetNullableFloat(parts, map, "distance_from_start_m"),
                        LateralErrorM = GetNullableFloat(parts, map, "lateral_error_m"),
                        DistanceFromShoreM = GetNullableFloat(parts, map, "distance_from_shore_m"),
                        MotorStatus = GetNullableBool(parts, map, "motor_status"),
                        LeakStatus = GetString(parts, map, "leak_status"),
                        Event = GetString(parts, map, "event"),
                        SuccessFlag = GetNullableBool(parts, map, "success_flag"),
                        SafeZoneOk = GetNullableBool(parts, map, "safe_zone_ok"),
                        PitchAngleOk = GetNullableBool(parts, map, "pitch_angle_ok"),
                        SurfaceOk = GetNullableBool(parts, map, "surface_ok"),
                        LaunchSignal = GetNullableBool(parts, map, "launch_signal")
                    };

                    result.Add(v);
                    sampleNo++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IMU dosyası parse hatası: {DosyaAdi}", dosyaAdi);
            }

            return result;
        }

        public async Task<string> DosyaKaydet(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "imu_data");
            Directory.CreateDirectory(folder);
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
        }

        private static bool TryParse(string? value, out double result)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
                   double.TryParse(value, NumberStyles.Float, CultureInfo.GetCultureInfo("tr-TR"), out result);
        }

        private static bool TryGetDouble(string[] parts, Dictionary<string, int> map, string key, out double value)
        {
            value = 0;
            return map.TryGetValue(key, out var i) && i < parts.Length && TryParse(parts[i], out value);
        }

        private static string GetString(string[] parts, Dictionary<string, int> map, string key)
        {
            return map.TryGetValue(key, out var i) && i < parts.Length ? parts[i] : string.Empty;
        }

        private static int? GetInt(string[] parts, Dictionary<string, int> map, string key)
        {
            return map.TryGetValue(key, out var i) && i < parts.Length && int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
        }

        private static float GetFloat(string[] parts, Dictionary<string, int> map, string key1, string? key2 = null, int? fallbackIndex = null)
        {
            var nullable = GetNullableFloat(parts, map, key1) ?? (key2 != null ? GetNullableFloat(parts, map, key2) : null);
            if (nullable.HasValue) return nullable.Value;
            if (fallbackIndex.HasValue && fallbackIndex.Value < parts.Length && TryParse(parts[fallbackIndex.Value], out var v)) return (float)v;
            return 0f;
        }

        private static float? GetNullableFloat(string[] parts, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out var i) || i >= parts.Length) return null;
            return TryParse(parts[i], out var v) ? (float)v : null;
        }

        private static bool? GetNullableBool(string[] parts, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out var i) || i >= parts.Length) return null;
            var value = parts[i].Trim();
            if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("OK", StringComparison.OrdinalIgnoreCase)) return true;
            if (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase) || value.Equals("FAIL", StringComparison.OrdinalIgnoreCase)) return false;
            return null;
        }
    }
}
