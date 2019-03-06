using System;

namespace LeanCloud.Realtime {
    internal class SessionHandler : CommandHandler {
        internal SessionHandler(Connection connection) : base(connection) {
        }

        internal override void Handle(GenericCommand command) {
            switch (command.Op) {
                case OpType.Closed:

                    break;
                default:
                    break;
            }
        }
    }
}
