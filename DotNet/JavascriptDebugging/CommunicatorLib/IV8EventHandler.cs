using CommunicatorLib.Messages;

namespace CommunicatorLib
{
    public interface IV8EventHandler
    {
        void Raise<TEvent>(TEvent e) where TEvent : IV8EventParameters;
    }
}