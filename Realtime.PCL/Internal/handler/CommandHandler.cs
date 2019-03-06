using System;

namespace LeanCloud.Realtime {
    internal abstract class CommandHandler {
        protected Connection Connection {
            get;
        }

        internal CommandHandler(Connection connection) {
            Connection = connection;
        }

        internal abstract void Handle(GenericCommand command);
    }
}
