using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        public static string ColorTag(this string str, ColorStringTag color)
        {
            string tag = color.ToString().ToLower();
            return string.Format("<color={0}>{1}</color>", tag, str);
        }

        #region JSON list helper

        public static string ToJson<T>(this List<T> list, bool prettyPrint = false)
        {
            return JsonUtility.ToJson(new JsonListHelper<T>(list), prettyPrint);
        }

        public static string ToJson<T>(this T[] array)
        {
            if (array == null) { throw new System.NullReferenceException(); }

            string json = JsonUtility.ToJson(new JsonArrayHelper<T>(array));

            var pos = json.IndexOf(":");
            json = json.Substring(pos + 1); // cut away "{ \"array\":"
            pos = json.LastIndexOf('}');
            json = json.Substring(0, pos - 1); // cut away "}" at the end
            return json;
        }

        public static void FromJson<T>(this List<T> list, string JSON)
        {
            if(list == null) { throw new System.NullReferenceException(); }

            JsonListHelper<T> retList = JsonUtility.FromJson<JsonListHelper<T>>(JSON);
            list.Clear();
            list.AddRange(retList.List);
        }

        public static T[] FromJson<T>(this T[] array, string JSON)
        {
            if (array == null) { throw new System.NullReferenceException(); }

            string newJson = "{ \"Array\": " + JSON + "}";
            JsonArrayHelper<T> retArray = JsonUtility.FromJson<JsonArrayHelper<T>>(newJson);

            return retArray.Array;
        }

        public static void AddFromJson<T>(this List<T> list, string JSON)
        {
            if (list == null) { throw new System.NullReferenceException(); }

            JsonListHelper<T> retList = JsonUtility.FromJson<JsonListHelper<T>>(JSON);
            list.AddRange(retList.List);
        }

        [System.Serializable]
        private struct JsonListHelper<T>
        {
            [SerializeField]
            public List<T> List;

            public JsonListHelper(List<T> list)
            {
                List = list;
            }

            public JsonListHelper(T[] array)
            {
                this.List = new List<T>(array);
            }
        }

        [System.Serializable]
        public struct JsonArrayHelper<T>
        {
            [SerializeField]
            public T[] Array;

            public JsonArrayHelper(T[] array)
            {
                Array = array;
            }
        }
        #endregion
    }
}
