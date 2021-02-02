using System.Collections.Generic;

namespace ArcSoftFace.Entity
{
    /// <summary>
    /// 字典控制实体类
    /// </summary>
    public class DictionaryUnit<T1, T2>
    {
        /// <summary>
        /// 锁
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// 字典
        /// </summary>
        private readonly Dictionary<T1, T2> dict = new Dictionary<T1, T2>();

        /// <summary>
        /// 获取字典的大小
        /// </summary>
        /// <returns></returns>
        public int GetDictCount()
        {
            lock (locker)
            {
                return dict.Count;
            }
        }

        /// <summary>
        /// 获取所有元素
        /// </summary>
        /// <returns></returns>
        public Dictionary<T1, T2> GetAllElement()
        {
            lock (locker)
            {
                return dict;
            }
        }

        /// <summary>
        /// 刷新元素，移除无用元素
        /// </summary>
        /// <param name="keyList">有效Key列表</param>
        public void RefershElements(List<T1> keyList)
        {
            lock (locker)
            {
                List<T1> tempKeyList = new List<T1>();
                tempKeyList.AddRange(dict.Keys);
                foreach (T1 key in tempKeyList)
                {
                    if (!keyList.Contains(key))
                    {
                        dict.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// 根据键获取某个值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public T2 GetElementByKey(T1 key)
        {
            lock (locker)
            {
                if (dict.ContainsKey(key))
                {
                    return dict[key];
                }
                else
                {
                    return default(T2);
                }
            }
        }

        /// <summary>
        /// 添加/更新元素
        /// </summary>
        /// <param name="key">元素-键</param>
        /// <param name="value">元素-值</param>
        public void AddDictionaryElement(T1 key,T2 value)
        {
            lock (locker)
            {
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                }
                else
                {
                    dict.Add(key, value);
                }
            }
        }

        /// <summary>
        /// 更新元素
        /// </summary>
        /// <param name="key">元素-键</param>
        /// <param name="value">元素-值</param>
        public void UpdateDictionaryElement(T1 key, T2 value)
        {
            lock (locker)
            {
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                }
            }
        }
        
        /// <summary>
        /// 移除所有元素
        /// </summary>
        public void ClearAllElement()
        {
            lock (locker)
            {
                dict.Clear();
            }
        }

        /// <summary>
        /// 判断是否包含之间的键
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public bool ContainsKey(T1 key)
        {
            lock (locker)
            {
                return dict.ContainsKey(key);
            }
        }
    }
}
