using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    public sealed class GlobalCoroutineSystem : MonoSingleton<GlobalCoroutineSystem>
    {
        public enum IOStatus
        {
            None,
            Wait,
            Running,

            [Obsolete("Use Running instead.")]
            Runing = Running,
        }

        public static class Config
        {
            public const int MAX_IO_COUNT = 10;
        }

        private readonly Dictionary<int, IEnumerator> waitingRoutines = new Dictionary<int, IEnumerator>();
        private readonly Queue<int> waitingOrder = new Queue<int>();
        private readonly Dictionary<int, Coroutine> runningRoutines = new Dictionary<int, Coroutine>();
        private int lastId = int.MinValue;
        private bool clearing;

        protected override bool IsDontDestroyOnLoad()
        {
            return true;
        }

        public int StartIO(IEnumerator routine)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var id = GetNewId();
            if (runningRoutines.Count >= Config.MAX_IO_COUNT)
            {
                waitingRoutines.Add(id, routine);
                waitingOrder.Enqueue(id);
            }
            else
            {
                StartRoutine(id, routine);
            }

            return id;
        }

        public void StopIO(int id)
        {
            if (waitingRoutines.Remove(id))
            {
                return;
            }

            if (!runningRoutines.TryGetValue(id, out var coroutine))
            {
                return;
            }

            runningRoutines.Remove(id);
            StopCoroutine(coroutine);
            StartWaitingRoutines();
        }

        public IOStatus GetIOState(int id)
        {
            if (waitingRoutines.ContainsKey(id))
            {
                return IOStatus.Wait;
            }

            return runningRoutines.ContainsKey(id) ? IOStatus.Running : IOStatus.None;
        }

        public void Clear()
        {
            clearing = true;
            StopAllCoroutines();
            waitingRoutines.Clear();
            waitingOrder.Clear();
            runningRoutines.Clear();
            clearing = false;
        }

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }

        private void StartRoutine(int id, IEnumerator routine)
        {
            runningRoutines.Add(id, StartCoroutine(RunRoutine(id, routine)));
        }

        private IEnumerator RunRoutine(int id, IEnumerator routine)
        {
            try
            {
                yield return routine;
            }
            finally
            {
                if (runningRoutines.Remove(id) && !clearing)
                {
                    StartWaitingRoutines();
                }
            }
        }

        private void StartWaitingRoutines()
        {
            while (runningRoutines.Count < Config.MAX_IO_COUNT && waitingOrder.Count > 0)
            {
                var id = waitingOrder.Dequeue();
                if (!waitingRoutines.Remove(id, out var routine))
                {
                    continue;
                }

                StartRoutine(id, routine);
            }
        }

        private int GetNewId()
        {
            do
            {
                lastId = lastId == int.MaxValue ? int.MinValue : lastId + 1;
            }
            while (waitingRoutines.ContainsKey(lastId) || runningRoutines.ContainsKey(lastId));

            return lastId;
        }
    }
}
