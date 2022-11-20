using Microsoft.IdentityModel.Tokens;

namespace API.Exceptions
{
    public class ExistsException : Exception
    {
        public string? Model { get; set; }
        public string? ProblematicField { get; set; }

        public override string Message => $"{Model} with same {ProblematicField} already exists";
    }

    public class UserExistsException : ExistsException
    {
       
        public UserExistsException(string field)
        {
            Model = "User";
            ProblematicField = field;
        }
    }

    public class FileExistsException : ExistsException
    {
        public override string Message => "File already exists";

    }
}
