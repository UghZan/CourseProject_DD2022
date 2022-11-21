using API.Exceptions;

namespace API.Exceptions
{
    public class NotFoundException : Exception
    {
        public string? AdditionalInfo { get; set; }
        public string? Model { get; set; }

        public override string Message => $"{Model} is not found: {AdditionalInfo}";
    }

    public class UserNotFoundException : NotFoundException
    {
        public UserNotFoundException()
        {
            Model = "User";
        }
        public UserNotFoundException(string parameter)
        {
            Model = "User";
            AdditionalInfo = parameter;
        }

    }

    public class PostNotFoundException : NotFoundException
    {
        public PostNotFoundException()
        {
            Model = "Post";
        }

    }
    public class CommentNotFoundException : NotFoundException
    {
        public CommentNotFoundException()
        {
            Model = "Comment";
        }

    }
    public class ReactionNotFoundException : NotFoundException
    {
        public ReactionNotFoundException()
        {
            Model = "Reaction";
        }
    }

    public class AttachNotFoundException : NotFoundException
    {

        public AttachNotFoundException()
        {
            Model = "Attach";
        }

    }

    public class SessionNotFoundException : NotFoundException
    {
        public override string Message => $"Session by that refresh token ID is not found";
    }
}
