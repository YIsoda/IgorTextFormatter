using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Text;

namespace IgorTextFormatter
{
    public class IgorTextFormatter
    {
        /// <summary>
        /// Writes 1-D data with the name, scales, unit names specified by <paramref name="waveDataInfo"/> to the <paramref name="file"/>.
        /// </summary>
        /// <param name="waveDataInfo"></param>
        /// <param name="waveData"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task WriteFile(WaveInfo waveDataInfo, IEnumerable<int> waveData, FileInfo file)
        {
            if (string.IsNullOrEmpty(waveDataInfo.Name))
            {
                throw new ArgumentException($"The name of Igor wave data, hense the {nameof(waveDataInfo.Name)} property, must not be empty string");
            }

            using var stringBuilder = ZString.CreateUtf8StringBuilder();
            Console.WriteLine(file.FullName);
            using var fs = File.Open(file.FullName, FileMode.OpenOrCreate);

            stringBuilder.AppendFormat(
                "IGOR\n" +
                "WAVES/D \'{0}\'\n" +
                "BEGIN\n", waveDataInfo.Name
                );

            stringBuilder.AppendJoin(separator: "\n", waveData);

            stringBuilder.AppendLine("END");

            stringBuilder.AppendFormat(@"X SetScale/P x {0},{1},""{2}"", '{6}'; SetScale y {3},{4},""{5}"", '{6}'",
                waveDataInfo.XScale.Start, waveDataInfo.XScale.Delta, waveDataInfo.XUnitName,
                waveDataInfo.YScale.Start, waveDataInfo.YScale.Delta, waveDataInfo.YUnitName, waveDataInfo.Name
                );

            await stringBuilder.WriteToAsync(fs);
        }
    }

    /// <summary>
    /// Represents the names, units, scalings of an Igor wave data.
    /// </summary>
    public class WaveInfo
    {
        //public ref Span<double> DataSpan;

        /// <summary>
        /// Represents the name of the wave. Must not be an empty string.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Represents the unit name in the x direction.
        /// </summary>
        /// <example>If the data is intensity vs wavelength, specifty the unit of wavelength, "nm".</example>
        public string? XUnitName { get; init; }

        /// <summary>
        /// Represents the unit name in the y direction.
        /// </summary>
        /// <example>If the data is intensity vs wavelength, specifty "nm".</example>
        public string? YUnitName { get; set; }

        /// <summary>
        /// Represents the scaling in the x direction.
        /// If not explicitly initialized, the default value is (<see cref="Scale.Start"/>, <see cref="Scale.Delta"/>) = (0, 1) (<see cref="Scale.Default"/>)
        /// </summary>
        /// <example></example>
        public Scale XScale { get; init; } = Scale.Default;

        /// <summary>
        /// Represents the scaling in the y direction.
        /// </summary>
        /// <example></example>
        public Scale YScale { get; set; }
    }

    /// <summary>
    /// Represents the scaling of Igor wave data.
    /// </summary>
    public class Scale
    {
        public static Scale FromStartAndEnd(double start, double end, int number) => new() { Start = start, Delta = (end - start) / (number + 1) };

        public static Scale FromStartAndDelta(double start, double delta) => new() { Start = start, Delta = delta };

        private Scale() { }

        public static Scale Default { get; } = new();

        public double Start { get; init; } = 0;

        public double Delta { get; init; } = 1;
    }
}
