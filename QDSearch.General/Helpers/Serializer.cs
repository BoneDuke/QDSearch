using System.IO;
using System.Text;
using System.Xml;

namespace QDSearch.Helpers
{
    /// <summary>
    /// Класс для сериализации объектов
    /// </summary>
    public static class XmlSerializer
    {
        /// <summary>
        /// Метод, сериализующий объект
        /// </summary>
        /// <param name="item">Объект для сериализации</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns></returns>
        public static string Serialize<T>(T item)
        {
            // todo: посмотреть внимательно что происходит
            var memStream = new MemoryStream();
            using (var textWriter = new XmlTextWriter(memStream, Encoding.Unicode))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(textWriter, item);

                memStream = textWriter.BaseStream as MemoryStream;
            }
            return memStream != null ? Encoding.Unicode.GetString(memStream.ToArray()) : null;
        }

        /// <summary>
        /// Десериализация объекта
        /// </summary>
        /// <param name="xmlString">Строка с сериализованным объектом</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
                return default(T);

            using (var memStream = new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(memStream);
            }
        }
    }
}
