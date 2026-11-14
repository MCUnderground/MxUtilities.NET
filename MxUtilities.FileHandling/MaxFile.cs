using MxUtilities.FileHandling.Constants;
using MxUtilities.FileHandling.Enums;
using MxUtilities.FileHandling.Helpers;
using MxUtilities.FileHandling.Models;
using OpenMcdf;
using OpenMcdf.Ole;
using System.Reflection;

namespace MxUtilities.FileHandling
{
    public class MaxFile : IDisposable
    {
        private readonly RootStorage _storage;
        private readonly MaxFileAccess _access;

        public MaxFile(string filename, MaxFileAccess access = MaxFileAccess.ReadOnly)
        {
            ValidateFileExists(filename);
            _storage = access == MaxFileAccess.ReadOnly
                ? RootStorage.OpenRead(filename)
                : RootStorage.Open(filename, FileMode.OpenOrCreate, StorageModeFlags.None);
            _access = access;
        }

        public RenderData? GetRenderData()
        {
            if (TryGetStream(MaxFileStreams.DocumentSummaryInformation, out MaxStream summaryStream))
            {
                return MaxFileParser.GetRenderData(summaryStream);
            }

            return null;
        }

        public MaxFileProperties? GetProperties()
        {
            if (_storage.TryOpenStream(MaxFileStreams.DocumentSummaryInformation, out var stream))
            {
                OlePropertiesContainer container = new(stream);
                List<OleProperty> properties = [];
                foreach (OleProperty property in container.Properties)
                {
                    properties.Add(property);
                }

                int offset = 0;
                List<object> names = properties[2].Value as List<object>;
                List<string> values = properties[3].Value as List<string>;

                var maxFileProperties = new MaxFileProperties();
                for (int i = 0; i < names.Count; i += 2)
                {
                    string propertyName = ((string)GetPropertyValue(names[i], "Value")).CleanString();
                    var valueCount = (int)GetPropertyValue(names[i + 1], "Value");

                    List<string> propertyValues = [];
                    for (int j = 0; j < valueCount; j++)
                    {
                        var value = values[offset + j].CleanString();
                        if (value.Contains(": "))
                        {
                            value = value.Split(": ")[1];
                        }

                        propertyValues.Add(value);
                    }

                    SetProperty(maxFileProperties, propertyName, propertyValues);
                    offset += valueCount;
                }
            }

            return null;
        }

        private void SetProperty(object target, string propertyName, object value)
        {
            propertyName = propertyName.Replace(" ", "");
            propertyName = propertyName.Replace("_", "");
            propertyName = propertyName.Replace("-", "");

            var property = target.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite)
            {
                return;
            }

            var valueConstructor = property.PropertyType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1);
            if (valueConstructor != null)
            {
                var parameterType = valueConstructor.GetParameters()[0].ParameterType;
                if (parameterType.IsAssignableFrom(value.GetType()))
                {
                    var instance = valueConstructor.Invoke([value]);
                    property.SetValue(target, instance);
                }
            }
            else
            {
                property.SetValue(target, value);
            }
        }


        public static object GetPropertyValue(object obj, string propertyName)
        {
            var type = obj.GetType();
            var propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj);
            }
            else
            {
                Console.WriteLine($"Property '{propertyName}' not found.");
                return null;
            }
        }


        public CfbStream GetStream(string streamName) => _storage.OpenStream(streamName);

        public bool TryGetStream(string streamName, out MaxStream maxStream)
        {
            if (!_storage.TryOpenStream(streamName, out var stream))
            {
                maxStream = null;
                return false;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                maxStream = new MaxStream(memoryStream.ToArray(), streamName);
                return true;
            }
        }

        public async Task SetStream(string streamName, MaxStream streamData)
        {
            if (_access == MaxFileAccess.ReadOnly)
                throw new Exception("Current MaxFile is set as readonly object.");

            _storage.Delete(streamName);

            var stream = _storage.CreateStream(streamName);
            var buffer = streamData.GetBuffer();
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
        }

        private static void ValidateFileExists(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException($"The file '{filename}' was not found.");
            }
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}
