using UnityEngine;

/// <summary>
/// CoexЭ�̵ĵȴ�����, ֱ��ʹ��Unity��Э�̵ȴ����ͽ��������κ�Ч��(ֻ���ӳ�һ֡, �൱��yield return null)
/// </summary>
namespace iCarus.Coex
{
    /// <summary>
    /// �ȴ�һ����ʱ��
    /// </summary>
    public class CoexWaitForSeconds : YieldInstruction
    {
        /// <summary>
        /// ����һ���ȴ�����
        /// </summary>
        /// <param name="seconds">�ȴ�ʱ��</param>
        /// <param name="ignoreTimescale">�Ƿ����ʱ��߶�(��������Ϸ��ͣ��ʱ����Ȼ���ּ�ʱ)</param>
        /// <returns>CoexWaitForSeconds</returns>
        public static CoexWaitForSeconds New(float seconds, bool ignoreTimescale = false)
        {
            return new CoexWaitForSeconds(seconds, ignoreTimescale);
        }

        CoexWaitForSeconds(float seconds, bool ignoreTimescale)
        { 
            mIgnoreTimescale = ignoreTimescale;
            mSeconds = seconds;
        }

        float mSeconds;
        float mTimeout;
        bool mIgnoreTimescale;

        internal bool timeout
        {
            get
            {
                return mIgnoreTimescale 
                    ? Time.unscaledTime >= mTimeout 
                    : Time.time >= mTimeout;
            }
        }

        internal void Reset()
        {
            mTimeout = mIgnoreTimescale ? Time.unscaledTime + mSeconds : Time.time + mSeconds;
        }
    }

    /// <summary>
    /// �ȴ�ֱ����ǰ֡����
    /// </summary>
    public class CoexWaitForEndOfFrame : YieldInstruction
    {
        /// <summary>
        /// ����һ���ȴ�����
        /// </summary>
        /// <returns>CoexWaitForEndOfFrame</returns>
        public static CoexWaitForEndOfFrame New() { return sInstance; }

        CoexWaitForEndOfFrame() { }
        static CoexWaitForEndOfFrame sInstance = new CoexWaitForEndOfFrame();
    }

    /// <summary>
    /// �ȴ�ֱ����һ�ε�FixedUpdate����
    /// </summary>
    public class CoexWaitForFixedUpdate : YieldInstruction
    {
        /// <summary>
        /// ����һ���ȴ�����
        /// </summary>
        /// <returns>CoexWaitForFixedUpdate</returns>
        public static CoexWaitForFixedUpdate New() { return sIntance; }

        CoexWaitForFixedUpdate() { }
        static CoexWaitForFixedUpdate sIntance = new CoexWaitForFixedUpdate();
    }
}