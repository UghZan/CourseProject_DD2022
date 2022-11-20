namespace API.Exceptions
{
    public class InvalidSessionException : Exception
    {
        public string ProblematicField { get; set; }
        public override string Message => $"Session invalid: {ProblematicField}";

        public InvalidSessionException(string problematicField)
        {
            ProblematicField = problematicField;
        }
    }

    public class InvalidLoginException : Exception
    {
        public string ProblematicField { get; set; }
        public override string Message => $"Login invalid: wrong {ProblematicField}";
        public InvalidLoginException(string problematicField)
        {
            ProblematicField = problematicField;
        }
    }
}
