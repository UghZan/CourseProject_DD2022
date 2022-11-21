namespace API.Exceptions
{
    public class PermissionException : Exception
    {
        public string Operation { get; set; }
        public override string Message => $"Insufficient permissions for operation: {Operation}";

        public PermissionException(string operation)
        {
            Operation = operation;
        }
    }
}
