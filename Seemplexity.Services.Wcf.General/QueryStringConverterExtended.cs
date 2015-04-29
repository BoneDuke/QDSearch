using System;
using System.Linq;
using System.ServiceModel.Dispatcher;

namespace Seemplexity.Services.Wcf
{
    /// <summary>
    /// This class converts a parameter in a query string to an object of the appropriate type. It can also convert a parameter from an object to its query string representation.
    /// It extended by support an arrays, System.Nullable type, Generic List
    /// </summary>
    public class QueryStringConverterExtended : QueryStringConverter
    {
        public override bool CanConvert(Type type)
        {
            if (type.IsArray)
                return base.CanConvert(type.GetElementType());
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                return base.CanConvert(type.GetGenericArguments()[0]);
            if (Nullable.GetUnderlyingType(type) != null)
                return base.CanConvert(Nullable.GetUnderlyingType(type));
            return base.CanConvert(type);
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (parameterType.IsArray)
            {
                var elementType = parameterType.GetElementType();
                var parameterList = parameter.Split(',');
                var result = Array.CreateInstance(elementType, parameterList.Length);
                for (var i = 0; i < parameterList.Length; i++)
                {
                    result.SetValue(base.ConvertStringToValue(parameterList[i], elementType), i);
                }

                return result;
            }
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                var elementType = parameterType.GetGenericArguments()[0];
                var objGenericList = Activator.CreateInstance(parameterType);
                var methodInfo = parameterType.GetMethod("Add");
                foreach (var result in parameter.Split(',').Select(str => base.ConvertStringToValue(str, elementType)))
                {
                    methodInfo.Invoke(objGenericList, new[] {result});
                }

                return objGenericList;
            }
            if (Nullable.GetUnderlyingType(parameterType) != null)
            {
                return (String.IsNullOrEmpty(parameter) ? null : base.ConvertStringToValue(parameter, Nullable.GetUnderlyingType(parameterType)));
            }
            if (parameterType.IsEnum)
            {
                return Enum.Parse(parameterType, parameter, true);
            }
            return base.ConvertStringToValue(parameter, parameterType);
        }

        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            if (parameterType.IsArray)
            {
                var array = (object[])parameter;
                return String.Join(",", array);
            }
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                var methodInfo = parameterType.GetMethod("ToArray");
                var array = (object[])methodInfo.Invoke(parameter, null); //todo проверить что данное приведение не вызовет ошибку если например масси имеет тип int[] а приводится к типу object[]
                return String.Join(",", array);
            }
            if (Nullable.GetUnderlyingType(parameterType) != null)
            {
                return base.ConvertValueToString(parameter, Nullable.GetUnderlyingType(parameterType));
            }

            return base.ConvertValueToString(parameter, parameterType);
        }

    }
}
