using Microsoft.Win32;
using System.Collections;
using System.Reflection;


namespace EasyRegistry {

    public static class RegistryConvert {

        //Serialization

        /// <summary>
        /// Serialized the object into the registry
        /// </summary>
        /// <param name="path">Serialization path</param>
        /// <param name="instance">Object instance</param>
        public static void Serialize(string path, object? instance) {
            switch (instance) {
                case IList:
                    Registry.LocalMachine.CreateSubKey(path + @"\" + instance.ToString(), true);
                    for (var i = 0; i < ((IList)instance).Count; i++) {
                        SerializeFromList(path + @"\" + instance.ToString(), ((IList)instance)[i], i);
                    }
                    break;
                case IDictionary:
                    var dicRegKey = Registry.LocalMachine.CreateSubKey(path + @"\" + instance.ToString(), true);
                    SerializeForDic((IDictionary)instance, dicRegKey);
                    break;
                case ValueType or string:
                    Registry.LocalMachine
                        .CreateSubKey(path, true)
                        .SetValue(instance.GetType().Name, instance.ToString());
                    break;
                default:
                    var classRegKey = Registry.LocalMachine.CreateSubKey(path + @"\" + instance.ToString(), true);
                    var instanceType = instance.GetType();
                    var propertyInfos = instanceType.GetProperties();
                    foreach (var item in propertyInfos) {
                        SerializeForClass(item.Name, item.GetValue(instance), classRegKey, path + @"\" + instance.ToString());
                    }
                    var fieldInfos = instanceType.GetFields();
                    foreach (var item in fieldInfos) {
                        SerializeForClass(item.Name, item.GetValue(instance), classRegKey, path + @"\" + instance.ToString());
                    }
                    break;
            }
        }

        /// <summary>
        /// Serialized the object into the registry, named with collection ordinal number
        /// </summary>
        /// <param name="path">Serialization path</param>
        /// <param name="instance">Object instance</param>
        /// <param name="elementNumber">The index number of the object in collection</param>
        static void SerializeFromList(string path, object? instance, int elementNumber) {
            Registry.LocalMachine.OpenSubKey(path, true).
                SetValue(elementNumber.ToString(), instance.ToString());
            switch (instance) {
                case IList:
                    Registry.LocalMachine.CreateSubKey(path + @"\" + elementNumber.ToString(), true);
                    for (var i = 0; i < ((IList)instance).Count; i++) {
                        SerializeFromList(path + @"\" + elementNumber.ToString(), ((IList)instance)[i], i);
                    }
                    break;
                case IDictionary:
                    var instanceRegKey = Registry.LocalMachine.CreateSubKey(path + @"\" + elementNumber.ToString(), true);
                    SerializeForDic((IDictionary)instance, instanceRegKey);
                    break;
                case ValueType or string:
                    Registry.LocalMachine
                        .CreateSubKey(path + @"\" + elementNumber.ToString(), true)
                        .SetValue(instance.GetType().Name, instance.ToString());
                    break;
                default:
                    var classRegKey = Registry.LocalMachine.CreateSubKey(path + @"\" + elementNumber.ToString(), true);
                    var instanceType = instance.GetType();
                    var propertyInfos = instanceType.GetProperties();
                    foreach (var item in propertyInfos) {
                        SerializeForClass(item.Name, item.GetValue(instance), classRegKey, path + @"\" + elementNumber.ToString());
                    }
                    var fieldInfos = instanceType.GetFields();
                    foreach (var item in fieldInfos) {
                        SerializeForClass(item.Name, item.GetValue(instance), classRegKey, path + @"\" + elementNumber.ToString());
                    }
                    break;
            }
        }

        /// <summary>
        /// Serialize the dictionary object into the registry
        /// </summary>
        /// <param name="instance">The dictionary instance</param>
        /// <param name="instanceRegKey">The subkey of the dictionary in the registry</param>
        static void SerializeForDic(IDictionary instance, RegistryKey instanceRegKey) {
            var keys = new ArrayList(instance.Keys);
            var values = new ArrayList(instance.Values);
            for (var i = 0; i < keys.Count; i++) {
                instanceRegKey.SetValue(keys[i].ToString(), values[i].ToString());
            }
        }

        /// <summary>
        /// Serialize the properties or fields of class objects into the registry
        /// </summary>
        /// <param name="itemName">The name of properties or field</param>
        /// <param name="value">The value of properties or field</param>
        /// <param name="instanceRegKey">Subkey in the registry for the class objects</param>
        /// <param name="path">Serialization path</param>
        static void SerializeForClass(string itemName, object? value, RegistryKey instanceRegKey, string path) {
            switch (value) {
                case null: return;
                case ValueType or string:
                    instanceRegKey.SetValue(itemName, value.ToString()); break;
                default:
                    instanceRegKey.SetValue(itemName, value.ToString());
                    Serialize(path, value); break;
            }
        }


        //Deserialization

        /// <summary>
        /// Deserialize registry and output the object instance
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>object instance</returns>
        public static T? Deserialize<T>(string path) {
            var type = typeof(T);
            if (type.IsArray) {
                return Array<T>(path);
            }
            else if (typeof(IList).IsAssignableFrom(type) && !typeof(ArrayList).IsAssignableFrom(type) && !type.IsArray) {//泛型集合
                return List<T>(path);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type)) {
                return Dictionary<T>(path);
            }
            else if (typeof(ArrayList).IsAssignableFrom(type)) {
                return ArrayList<T>(path);
            }
            else if (type.IsValueType || typeof(string).IsAssignableFrom(type)) {
                return ValueOrString<T>(path);
            }
            return Class<T>(path);
        }

        /// <summary>
        /// Creates array instance for output
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>Array instance</returns>
        static T? Array<T>(string path) {
            var elementType = typeof(T).GetElementType();
            var elementsCount = Registry.LocalMachine.OpenSubKey(path).GetSubKeyNames().Length;
            var t = System.Array.CreateInstance(elementType, elementsCount);
            for (var i = 0; i < elementsCount; i++) {
                var mi = typeof(RegistryConvert).GetMethod("Deserialize")?.MakeGenericMethod(elementType);
                t?.SetValue(mi?.Invoke(null, new object[] { path + @"\" + i.ToString() }), i);
            }
            var tempT = (object?)t;
            return (T?)tempT;
        }
        /// <summary>
        /// Creates generic list instance for output
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>generic list instance</returns>
        static T? List<T>(string path) {
            var elementType = typeof(T).GetGenericArguments()[0];
            var elementsCount = Registry.LocalMachine.OpenSubKey(path).GetSubKeyNames().Length;
            var tempT = (IList?)Activator.CreateInstance(typeof(T));
            for (var i = 0; i < elementsCount; i++) {
                var mi = typeof(RegistryConvert).GetMethod("Deserialize").MakeGenericMethod(elementType);
                tempT?.Add(mi.Invoke(null, new object[] { path + @"\" + i.ToString() }));
            }
            return (T?)tempT;
        }

        /// <summary>
        /// Creates dictionary instance for output
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>dictionary instance</returns>
        static T? Dictionary<T>(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var valueNames = regKey.GetValueNames();
            var tempT = (IDictionary?)Activator.CreateInstance(typeof(T));
            for (var i = 0; i < valueNames.Length; i++) {
                tempT.Add(valueNames[i], regKey.GetValue(valueNames[i]));
            }
            return (T?)tempT;
        }

        /// <summary>
        /// Creates arraylist instance for output
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>Arraylist instance</returns>
        static T? ArrayList<T>(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var elementsCount = regKey.GetSubKeyNames().Length;
            var tempT = (IList?)Activator.CreateInstance(typeof(T));
            for (var i = 0; i < elementsCount; i++) {
                tempT.Add(regKey.GetValue(i.ToString()));
            }
            return (T?)tempT;
        }

        /// <summary>
        /// Creates valueType instance for output
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>valueType instance</returns>
        static T? ValueOrString<T>(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var value = regKey.GetValue(typeof(T).Name);
            if (typeof(T).IsEnum) {
                var t1 = Enum.Parse(value.GetType(), value.ToString());
                return (T?)t1;
            }
            var t = Convert.ChangeType(value, typeof(T));
            return (T?)t;
        }

        /// <summary>
        /// Creates the class instance for output
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="path">Deserialization path</param>
        /// <returns>class instance</returns>
        static T? Class<T>(string path) {
            var type = typeof(T);
            var t = Activator.CreateInstance(typeof(T));
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var valueNames = regKey.GetValueNames();
            var propertyInfos = type.GetProperties();
            var fieldInfos = type.GetFields();
            Property(valueNames, path, propertyInfos, t, regKey);
            return (T?)Field(valueNames, path, fieldInfos, t, regKey);
        }

        /// <summary>
        /// Deserialize and output property values in class instance
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="valueNames">The array containing value names of registry subkey</param>
        /// <param name="path">Deserialization path</param>
        /// <param name="propertyInfos">Deserialization of the output instance property values</param>
        /// <param name="t">Deserialized instance of output</param>
        /// <param name="regKey">The registry subkey to deserialize</param>
        /// <returns>class instance</returns>
        static T Property<T>(string[] valueNames, string path, PropertyInfo[] propertyInfos, T t, RegistryKey regKey) {
            foreach (var item in propertyInfos) {
                for (var i = 0; i < valueNames.Length; i++) {
                    if (item.Name == valueNames[i]) {
                        var regValue = regKey.GetValue(valueNames[i]);
                        var propertyType = item.PropertyType;
                        if (propertyType.IsEnum) {
                            item.SetValue(t, Enum.Parse(propertyType, regValue.ToString()));
                        }
                        else if ((propertyType.IsValueType && !propertyType.IsEnum) || propertyType == typeof(string)) {
                            item.SetValue(t, Convert.ChangeType(regValue, propertyType));
                        }
                        else {
                            var value = typeof(RegistryConvert)
                                .GetMethod("Deserialize")?
                                .MakeGenericMethod(propertyType)
                                .Invoke(null, new object[] { path + @"\" + regValue.ToString() });
                            item.SetValue(t, value);
                        }
                        #region Exclude the key from the array to prevent double alignment
                        var tempList = valueNames.ToList();
                        tempList.RemoveAt(i);
                        if (tempList.Count == 0) {
                            return t;
                        }
                        valueNames = tempList.ToArray();
                        #endregion
                        break;
                    }
                }
            }
            return t;
        }

        /// <summary>
        /// Deserialize and output Field values in class instance
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="valueNames">The array containing value names of registry subkey</param>
        /// <param name="path">Deserialization path</param>
        /// <param name="fieldInfos">Deserialization of the output instance Fields values</param>
        /// <param name="t">class instance for output</param>
        /// <param name="regKey">The registry subkey to deserialize</param>
        /// <returns>class instance</returns>
        static T Field<T>(string[] valueNames, string path, FieldInfo[] fieldInfos, T t, RegistryKey regKey) {
            foreach (var item in fieldInfos) {
                for (var i = 0; i < valueNames.Length; i++) {
                    if (item.Name == valueNames[i]) {
                        var regValue = regKey.GetValue(valueNames[i]);
                        var fieldType = item.FieldType;
                        if (fieldType.IsEnum) {
                            item.SetValue(t, Enum.Parse(fieldType, regValue.ToString()));
                        }
                        else if ((fieldType.IsValueType && !fieldType.IsEnum) || fieldType == typeof(string)) {
                            item.SetValue(t, Convert.ChangeType(regValue, fieldType));
                        }
                        else {
                            var value = typeof(RegistryConvert)
                                .GetMethod("Deserialize")?
                                .MakeGenericMethod(fieldType)
                                .Invoke(null, new object[] { path + @"\" + regValue.ToString() });
                            item.SetValue(t, value);
                        }
                        #region Exclude the key from the array to prevent double alignment
                        var tempList = valueNames.ToList();
                        tempList.RemoveAt(i);
                        if (tempList.Count == 0) {
                            return t;
                        }
                        valueNames = tempList.ToArray();
                        #endregion
                        break;
                    }
                }
            }
            return t;
        }
    }
}