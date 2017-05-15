namespace Roidis.Exception
{
    public class RoidException : System.Exception
    {
        public RoidException() : base()
        {
        }

        public RoidException(string message) : base(message)
        {
        }

        public RoidException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}