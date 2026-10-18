using System;

namespace ChaosHouse.Core
{
    public class HouseEventArgs : EventArgs
    {
        public HouseMember Character { get; }
        public string Message { get; }

        public HouseEventArgs(HouseMember character, string message)
        {
            Character = character;
            Message = message;
        }
    }
}
