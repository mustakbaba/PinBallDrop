using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ElephantSDK;
using UnityEngine;
using UnityEngine.Events;

namespace SincappStudio
{
    public static class Sincapp
    {
        public static void LookAtSmoothly(this Transform transform, GameObject target, float smoothness)
        {
            Vector3 direction = target.transform.position - transform.position;
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), smoothness);
        }

        public static void SetX_Pos(this Transform transform, float value, Space spaceSelforWorld = Space.World)
        {
            if (spaceSelforWorld == Space.World)
            {
                var position = transform.position;
                position = new Vector3(value, position.y, position.z);
                transform.position = position;
            }
            else
            {
                var position = transform.localPosition;
                position = new Vector3(value, position.y, position.z);
                transform.localPosition = position;
            }
        }

        public static void SetY_Pos(this Transform transform, float value, Space spaceSelforWorld = Space.World)
        {
            if (spaceSelforWorld == Space.World)
            {
                var position = transform.position;
                position = new Vector3(position.x, value, position.z);
                transform.position = position;
            }
            else
            {
                var position = transform.localPosition;
                position = new Vector3(position.x, value, position.z);
                transform.localPosition = position;
            }
        }

        public static void SetZ_Pos(this Transform transform, float value, Space spaceSelforWorld = Space.World)
        {
            if (spaceSelforWorld == Space.World)
            {
                var position = transform.position;
                position = new Vector3(position.x, position.y, value);
                transform.position = position;
            }
            else
            {
                var position = transform.localPosition;
                position = new Vector3(position.x, position.y, value);
                transform.localPosition = position;
            }
        }
        
        public static IEnumerator WaitAndAction(float delayTime, UnityAction action)
        {
            yield return new WaitForSeconds(delayTime);
            action?.Invoke();
        }
        
        public static float[] StringListToFloatArray(string stringListKey,string defaultValues)
        {
            string stringList = RemoteConfig.GetInstance().Get(stringListKey,defaultValues);
            string[] pureList = stringList.Split('-');
            float[] targetValues = new float[pureList.Length];

            for (int i = 0; i < targetValues.Length; i++)
            {
                targetValues[i] = float.Parse(pureList[i],NumberStyles.Number, CultureInfo.InvariantCulture);
            }

            return targetValues;
        }
        
        public static int[] StringListToIntArray(string stringListKey,string defaultValues)
        {
            string stringList = RemoteConfig.GetInstance().Get(stringListKey,defaultValues);
            string[] pureList = stringList.Split('-');
            int[] targetValues = new int[pureList.Length];

            for (int i = 0; i < targetValues.Length; i++)
            {
                targetValues[i] = int.Parse(pureList[i],NumberStyles.Number, CultureInfo.InvariantCulture);
            }

            return targetValues;
        }

        private static readonly SortedDictionary<float, string> abbrevations = new SortedDictionary<float, string>
        {
            {1000, "K"},
            {1000000, "M"},
            {1000000000, "B"},
            {1000000000000, "T"}
        };

        public static string AbbreviateNumber(float number, string stringFormat = "F0")
        {
            for (int i = abbrevations.Count - 1; i >= 0; i--)
            {
                KeyValuePair<float, string> pair = abbrevations.ElementAt(i);

                if (Mathf.Abs(number) >= pair.Key)
                {
                    float roundedNumber = number / pair.Key;

                    if (Mathf.Round(roundedNumber) == roundedNumber)
                    {
                        return roundedNumber.ToString("0") + pair.Value;
                    }
                    else
                    {
                        return roundedNumber.ToString("F1", CultureInfo.InvariantCulture) + pair.Value;
                    }
                }
            }

            return number.ToString(stringFormat);
        }
    }
}