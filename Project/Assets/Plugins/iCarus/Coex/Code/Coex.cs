using UnityEngine;
using System;
using System.Collections;

namespace iCarus.Coex
{
    public class CoexException : Exception { }

    /// <summary>
    /// coroutine extensionЭ��, �̳���CoexBehaviour�Ľű�����StartCoroutine�󷵻ص�Э��object
    /// </summary>
    public class Coex
    {
        /// <summary>
        /// Э�̵�ִ��״̬
        /// </summary>
        public enum State
        {
            Running,
            Interrupted,
            Error,
            Done,
        }

        // �Ƿ�ӵ�з���ֵ
        public bool hasReturnValue { get { return mReturnValueType != null; } }
        // �Ѿ�ִ�е�yield����
        public int yieldCount { get { return mYieldCount; } }
        // ִ��״̬
        public State state { get { return mState; } }

        /// <summary>
        /// ��ִ���쳣���, ���û���쳣, ȡЭ��ִ�еķ���ֵ
        /// </summary>
        /// <typeparam name="T">����ֵ����, ��Ҫ�͵���StartCoroutine<T>���������һ��</typeparam>
        /// <returns>Э��ִ�еķ���ֵ</returns>
        /// <exception>���ִ�й����в������쳣, ��ε��ý����׳��Ǹ��쳣</exception>
        /// <exception cref="CoroutineNoReturnValueException">���Э��û�з���ֵ, ���ý����׳��쳣</exception>
        public T ReturnValue<T>()
        {
            if (null == mReturnValueType)
                Exception.Throw<CoexException>("trying to access the return value of a coroutine which hasn't yield one");
            if (null != mException)
                throw mException;

            return (T)mReturnValue;
        }

        /// <summary>
        /// ���Э��ִ�еĹ������Ƿ����쳣����
        /// </summary>
        /// <exception>���ִ�й����в������쳣, ��ε��ý����׳��Ǹ��쳣</exception>
        public void CheckError()
        {
            if (null != mException)
                throw mException;
        }

        #region internal
        IEnumerator mRoutine;
        object mReturnValue;
        Type mReturnValueType;
        Exception mException;
        int mYieldCount;
        State mState;

        internal object returnValue { get { return mReturnValue; } }
        internal string routineName { get { return mRoutine.ToString(); } }
        internal Type returnValueType { get { return mReturnValueType; } }
        internal bool foldout;

        internal CoexEngine.Process process = CoexEngine.Process.None;
        internal int frameCount = 0;

        internal Coex(IEnumerator routine, Type returnValueType)
        {
            mRoutine = routine;
            mReturnValue = null;
            mReturnValueType = returnValueType;
            mException = null;
            mYieldCount = 0;
            mState = State.Running;
            frameCount = Time.frameCount;
        }

        internal void Interrupt()
        {
            if (mState == State.Running)
                mState = State.Interrupted;
        }

        internal void MoveNext()
        {
            try
            {
                MoveNextExceptional();
            }
            catch (Exception e)
            {
                mException = e;
                mState = State.Error;
            }
            frameCount = Time.frameCount;
        }

        internal void MoveNextExceptional()
        {
            if (!mRoutine.MoveNext())
            {
                mState = State.Done;
                return;
            }

            ++mYieldCount;
            mReturnValue = mRoutine.Current;
            if (null != mReturnValueType && 
                null != mReturnValue && 
                mReturnValue.GetType() == mReturnValueType)
            {
                mState = State.Done;
            }
            else if (null != mReturnValue)
            {
                if (mReturnValue is CoexWaitForSeconds)
                    ((CoexWaitForSeconds)mReturnValue).Reset();
            }
        }
        #endregion internal
    }
}