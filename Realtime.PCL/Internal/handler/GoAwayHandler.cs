using System;

namespace LeanCloud.Realtime {
    internal class GoAwayHandler : CommandHandler {
        internal GoAwayHandler(Connection connection) : base(connection) {

        }

        internal override void Handle(GenericCommand command) {
            Connection.ClearCache().ContinueWith(t => { 
                if (t.IsFaulted) {
                    AVRealtime.PrintLog(t.Exception.InnerException.Message);
                    return;
                }
                Connection.Disconnect();
            });
        }
    }
}
