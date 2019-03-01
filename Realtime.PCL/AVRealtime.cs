using System;

namespace LeanCloud.Realtime {
    public class AVRealtime {
        public AVRealtime() {
            var command = new SessionCommand { 
                
            };
        }


        public static Action<string> LogPrinter {
            private get; set; 
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="log">日志</param>
        public static void PrintLog(string log) {
            // TODO 通过代理输出
            LogPrinter?.Invoke(log);
        }

        public static void PrintLog(string format, params object[] args) {
            string log = string.Format(format, args);
            PrintLog(log);
        }
    }
}
