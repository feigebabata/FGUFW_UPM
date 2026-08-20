namespace FGUFW
{
    public static partial class fg
    {
        public static IOrderedMessenger<string> messenger { get; private set; } = new OrderedMessenger<string>();
    }
}