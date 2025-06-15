using System;
using System.Collections.Generic;
using System.Threading;

namespace Engine
{
	public static class Dispatcher
	{
        public struct ActionInfo
		{
			public Action Action;

			public ManualResetEventSlim Event;
		}

		private static int? m_mainThreadId;

		private static List<ActionInfo> m_actionInfos = [];

		private static List<ActionInfo> m_currentActionInfos = [];

		public static int MainThreadId
		{
			get
			{
                return m_mainThreadId ?? throw new InvalidOperationException("Dispatcher is not initialized.") ;
            }
        }

        public static void ExecuteActionsOnCurrentThread()
        {
            m_currentActionInfos.Clear();
            lock (m_actionInfos)
            {
                m_currentActionInfos.AddRange(m_actionInfos);
                m_actionInfos.Clear();
            }
            foreach (ActionInfo currentActionInfo in m_currentActionInfos)
            {
                try
                {
                    currentActionInfo.Action();
                }
                catch (Exception ex)
                {
                    Log.Error("Dispatched action failed. Reason: {0}", ex);
                }
                finally
                {
                    if (currentActionInfo.Event != null)
                    {
                        currentActionInfo.Event.Set();
                    }
                }
            }
        }

		public static void Dispatch(Action action, bool waitUntilCompleted = false)
		{
			if (!m_mainThreadId.HasValue)
			{
				throw new InvalidOperationException("Dispatcher is not initialized.");
			}
			ActionInfo actionInfo;
			if (m_mainThreadId.Value == Environment.CurrentManagedThreadId)
			{
				action();
			}
			else if (waitUntilCompleted)
			{
				actionInfo = default(ActionInfo);
				actionInfo.Action = action;
				actionInfo.Event = new ManualResetEventSlim(initialState: false);
				ActionInfo item = actionInfo;
				lock (m_actionInfos)
				{
					m_actionInfos.Add(item);
				}
				item.Event.Wait();
				item.Event.Dispose();
			}
			else
			{
				lock (m_actionInfos)
				{
					List<ActionInfo> actionInfos = m_actionInfos;
					actionInfo = new ActionInfo
					{
						Action = action
					};
					actionInfos.Add(actionInfo);
				}
			}
		}

		internal static void Initialize()
		{
			m_mainThreadId = Environment.CurrentManagedThreadId;
		}

		internal static void BeforeFrame()
		{
            ExecuteActionsOnCurrentThread();
		}

		internal static void AfterFrame()
		{
		}
	}
}