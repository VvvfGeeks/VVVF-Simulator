using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace VvvfSimulator.Data.TrainAudio
{
    public class Manager
    {
        public static bool Save(string path, Struct Data, bool UseException = false)
        {
            try
            {
                TextWriter writer = System.IO.File.CreateText(path);
                new Serializer().Serialize(writer, Data);
                writer.Close();
                return true;
            }
            catch (Exception)
            {
                if (UseException)
                    throw;
                else
                    return false;
            }
        }
        public static Struct? Load(string Path, bool UseException = false)
        {
            try
            {
                StreamReader reader = new StreamReader(Path, Encoding.UTF8);
                Struct deserializeObject = new Deserializer().Deserialize<Struct>(reader);
                reader.Close();
                return deserializeObject;
            }
            catch
            {
                if (UseException)
                    throw;
                else
                    return null;
            }
        }

        public static Struct Template { get; } = new();
        public static Struct? LoadData { get; set; } = null;
        public static Struct Current { get; set; } = DeepClone(Template);
        public static string LoadPath { get; set; } = string.Empty;
        public static void LoadCurrent(string Path)
        {
            try
            {
                Struct? Result = Load(Path, true);
                if (Result == null)
                    return;
                Current = Result;
                LoadPath = Path;
                LoadData = DeepClone(Current);
            }
            catch
            {
                throw;
            }
        }
        public static bool SaveCurrent(string Path)
        {
            bool Result = Save(Path, Current);
            if (Result)
            {
                LoadPath = Path;
                LoadData = DeepClone(Current);
            }
            return Result;
        }
        public static void ResetCurrent()
        {
            Current = DeepClone(Template);
            LoadPath = string.Empty;
            LoadData = null;
        }
        public static bool IsCurrentEquivalent(Struct? X)
        {
            if (X == null) return false;
            return IsEquivalent(Current, X);
        }
        public static Struct DeepClone(Struct Source)
        {
            return new Deserializer().Deserialize<Struct>(new Serializer().Serialize(Source));
        }
        public static bool IsEquivalent(Struct X, Struct Y)
        {
            if (ReferenceEquals(X, Y)) return true;
            return new Serializer().Serialize(X).Equals(new Serializer().Serialize(Y));
        }
        public static string GetLoadedYamlName()
        {
            return Path.GetFileNameWithoutExtension(LoadPath);
        }
    }
}
