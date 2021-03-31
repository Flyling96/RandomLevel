using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DragonSlay
{
    public static class SerializationHelper
    {
        [Serializable]
        public struct TypeSerializationInfo
        {
            [SerializeField]
            public string fullName;

            public bool IsValid()
            {
                return !string.IsNullOrEmpty(fullName);
            }
        }

        [Serializable]
        public struct JSONSerializedElement
        {
            [SerializeField]
            public TypeSerializationInfo typeInfo;

            [SerializeField]
            public string JSONnodeData;
        }

        public static JSONSerializedElement nullElement
        {
            get
            {
                return new JSONSerializedElement();
            }
        }

        public static TypeSerializationInfo GetTypeSerializableAsString(Type type)
        {
            return new TypeSerializationInfo
            {
                fullName = type.FullName
            };
        }

        static Type GetTypeFromSerializedString(TypeSerializationInfo typeInfo)
        {
            if (!typeInfo.IsValid())
                return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeInfo.fullName);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static JSONSerializedElement Serialize<T>(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "Can not serialize null element");

            var typeInfo = GetTypeSerializableAsString(item.GetType());
            var data = JsonUtility.ToJson(item, true);

            if (string.IsNullOrEmpty(data))
                throw new ArgumentException(string.Format("Can not serialize {0}", item));
            ;

            return new JSONSerializedElement
            {
                typeInfo = typeInfo,
                JSONnodeData = data
            };
        }

        static TypeSerializationInfo DoTypeRemap(TypeSerializationInfo info, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper)
        {
            TypeSerializationInfo foundInfo;
            if (remapper.TryGetValue(info, out foundInfo))
                return foundInfo;
            return info;
        }

        public static T Deserialize<T>(JSONSerializedElement item, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper,  params object[] constructorArgs) where T : class
        {
            if (!item.typeInfo.IsValid() || string.IsNullOrEmpty(item.JSONnodeData))
            {//throw new ArgumentException(string.Format("Can not deserialize {0}, it is invalid", item));
                return null;
            }

            TypeSerializationInfo info = item.typeInfo;
            info.fullName = info.fullName.Replace("UnityEngine.MaterialGraph", "UnityEditor.ShaderGraph");
            info.fullName = info.fullName.Replace("UnityEngine.Graphing", "UnityEditor.Graphing");
            if (remapper != null)
                info = DoTypeRemap(info, remapper);

            var type = GetTypeFromSerializedString(info);
            if (type == null)
                throw new ArgumentException(string.Format("Can not deserialize ({0}), type is invalid", info.fullName));

            T instance;
            try
            {
                var culture = CultureInfo.CurrentCulture;
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                instance = Activator.CreateInstance(type, flags, null, constructorArgs, culture) as T;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Could not construct instance of: {0}", type), e);
            }

            if (instance != null)
            {
                JsonUtility.FromJsonOverwrite(item.JSONnodeData, instance);
                return instance;
            }
            return null;
        }

        public static List<JSONSerializedElement> Serialize<T>(IEnumerable<T> list)
        {
            var result = new List<JSONSerializedElement>();
            if (list == null)
                return result;

            foreach (var element in list)
            {
                try
                {
                    result.Add(Serialize(element));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return result;
        }

        public static List<T> Deserialize<T>(IEnumerable<JSONSerializedElement> list, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper, params object[] constructorArgs) where T : class
        {
            var result = new List<T>();
            if (list == null)
                return result;

            foreach (var element in list)
            {
                try
                {
                    result.Add(Deserialize<T>(element, remapper));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError(element.JSONnodeData);
                }
            }
            return result;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }
#endif

    }
}
