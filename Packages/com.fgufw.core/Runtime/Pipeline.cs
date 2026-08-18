using System;

namespace FGUFW
{
    public abstract class Pipeline<CONTEXT>
    {
        public interface IPipe
        {
            /// <summary>
            /// 处理
            /// </summary>
            /// <param name="context">上下文</param>
            /// <returns>是否中断</returns>
            bool Execute(ref CONTEXT context);
        }

        protected IPipe[] _pipes;

        public IPipe this[Enum e]
        {
            get
            {
                var idx = e.GetHashCode();
                return _pipes[idx];
            }
            set
            {
                var idx = e.GetHashCode();
                _pipes[idx] = value;
            }
        }

        public virtual void Execute(ref CONTEXT context)
        {
            foreach (var pipe in _pipes)
            {
                if(pipe != default && pipe.Execute(ref context))break;
            }
        }
    }
}