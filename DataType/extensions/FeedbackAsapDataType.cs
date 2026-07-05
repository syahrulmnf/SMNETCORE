using SMNETCORE.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SMNETCORE.DataType.Extensions
{
    public static class FeedbackAsapDataType
    {
        public static DataTypeEnum GetEnumVariableType(PropertyInfo propInfo)
        {
            return GetEnumVariableType(propInfo.PropertyType);
        }

        public static DataTypeEnum GetEnumVariableType(Type dataType)
        {
            var retVal = DataTypeEnum.String;
            if (typeof(DateTime).Equals(dataType))
            {
                retVal = DataTypeEnum.Date;
            }
            else if (typeof(Boolean).Equals(dataType))
            {
                retVal = DataTypeEnum.Boolean;
            }
            else if (typeof(int).Equals(dataType))
            {
                retVal = DataTypeEnum.Integer;
            }
            else if (typeof(Double).Equals(dataType))
            {
                retVal = DataTypeEnum.Double;
            }
            else if (typeof(float).Equals(dataType))
            {
                retVal = DataTypeEnum.Float;
            }
            else if (typeof(decimal).Equals(dataType))
            {
                retVal = DataTypeEnum.Decimal;
            }
            return retVal;
        }

        public static void ConvertValue<T>(this string value, DataTypeEnum dataType, bool isList, Type enumTypeValue, out T tryValue)
        {
            object tempTryValue = default(T);
            ConvertType(value, dataType, isList, enumTypeValue, out tempTryValue);
            tryValue = tempTryValue.To<T>().Value;
        }

        public static void ConvertValue<T>(this string value, DataTypeEnum dataType, bool isList, out T tryValue)
        {
            object tempTryValue = default(T);
            ConvertType(value, dataType, isList, out tempTryValue);
            tryValue = tempTryValue.To<T>().Value;
        }

        public static void ConvertType(this object value, DataTypeEnum dataType, bool isList, out object tryValue)
        {
            if (isList)
            {
                var data = value.To<string>().Value.Split(',');
                List<object> dataList = new List<object>();
                foreach (var cData in data)
                {
                    object tObject;
                    cData.ConvertType(dataType, false, out tObject);
                    dataList.Add(tObject);
                }
                tryValue = dataList;
                return;
            }
            switch (dataType)
            {
                
                case DataTypeEnum.Boolean:
                    bool tryBool;
                    bool.TryParse((string)value, out tryBool);
                    tryValue = tryBool;
                    break;
                case DataTypeEnum.Integer:
                    int tryInt;
                    int.TryParse((string)value, out tryInt);
                    tryValue = tryInt;
                    break;
                case DataTypeEnum.Double:
                    Double tryDouble;
                    Double.TryParse((string)value, out tryDouble);
                    tryValue = tryDouble;
                    break;
                case DataTypeEnum.Float:
                    float tryFloat;
                    float.TryParse((string)value, out tryFloat);
                    tryValue = tryFloat;
                    break;
                case DataTypeEnum.Decimal:
                    float tryDecimal;
                    float.TryParse((string)value, out tryDecimal);
                    tryValue = tryDecimal;
                    break;
                case DataTypeEnum.Date:
                    DateTime tryDate;
                    Common.Helpers.Utils.TryConvertGenericStringToDate((string)value, out tryDate);
                    tryValue = tryDate;
                    break;
                case DataTypeEnum.String:
                default:
                    tryValue = value;
                    break;
            }
        }

        public static void ConvertType(this object value, DataTypeEnum dataType, bool isList, Type enumTypeValue, out object tryValue)
        {
            if (isList)
            {
                var data = value.To<string>().Value.Split(',');
                List<object> dataList = new List<object>();
                foreach (var cData in data)
                {
                    object tObject;
                    cData.ConvertType(dataType, false, enumTypeValue, out tObject);
                    dataList.Add(tObject);
                }
                tryValue = dataList;
                return;
            }
            switch (dataType)
            {
                case DataTypeEnum.Enum:
                    if (value.GetType() == enumTypeValue)
                    {
                        tryValue = Convert.ChangeType(value, enumTypeValue);
                    }
                    else
                    {
                        //Note: The following line is because Convert.ChangeType() fails for nullable types.
                        var destinationType = Nullable.GetUnderlyingType(enumTypeValue) ?? enumTypeValue;
                        tryValue = Convert.ChangeType(value, destinationType);
                    }
                    break;
                default:
                    ConvertType( value, dataType, isList, out tryValue);
                    break;
            }
        }

        public static Type GetEnumVariableType(DataTypeEnum dataType, string Name)
        {
            switch (dataType)
            {
                case DataTypeEnum.Enum:
                    return Enums.GetEnumType(Name);
                case DataTypeEnum.Boolean:
                    return typeof(Boolean);
                case DataTypeEnum.Integer:
                    return typeof(int);
                case DataTypeEnum.Double:
                    return typeof(Double);
                case DataTypeEnum.Float:
                    return typeof(float);
                case DataTypeEnum.Decimal:
                    return typeof(decimal);
                case DataTypeEnum.String:
                default:
                    return typeof(String);
            }
        }
    }
}
