using System;

namespace ElephantSocial.Core
{
    public class ElephantSocialException : Exception
    {
        public ElephantSocialException(string message) : base(message) { }
        public ElephantSocialException(string message, Exception inner) : base(message, inner) { }
    }
    
    public class ConnectionException : ElephantSocialException
    {
        public ConnectionException(string message) : base(message) { }
        public ConnectionException(string message, Exception inner) : base(message, inner) { }
    }
    
    public class TeamOperationException : ElephantSocialException
    {
        public TeamOperationException(string message) : base(message) { }
        public TeamOperationException(string message, Exception inner) : base(message, inner) { }
    }
    
    public class ChatOperationException : ElephantSocialException
    {
        public ChatOperationException(string message) : base(message) { }
        public ChatOperationException(string message, Exception inner) : base(message, inner) { }
    }
}