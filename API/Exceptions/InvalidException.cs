namespace API.Exceptions
{
    public class InvalidException : Exception
    {
        protected string ProblematicField { get; set; }
    }
    public class InvalidSessionException : InvalidException
    {
        
        public override string Message => $"Session invalid: {ProblematicField}";

        public InvalidSessionException(string problematicField)
        {
            ProblematicField = problematicField;
        }
    }

    public class InvalidReactionException : InvalidException
    {
        public override string Message => $"Invalid reaction id: {ProblematicField}";

        public InvalidReactionException(string problematicField)
        {
            ProblematicField = problematicField;
        }
    }

    public class InvalidLoginException : InvalidException
    {
        public override string Message => $"Login invalid: wrong {ProblematicField}";
        public InvalidLoginException(string problematicField)
        {
            ProblematicField = problematicField;
        }
    }

    public class InvalidOperationException : InvalidException
    {
        public override string Message => $"Invalid operation: {ProblematicField}";
        public InvalidOperationException(string problematicField)
        {
            ProblematicField = problematicField;
        }
    }
}
