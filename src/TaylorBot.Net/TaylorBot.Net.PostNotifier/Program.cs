namespace TaylorBot.Net.PostNotifier
{
    class Program
    {
        static void Main(string[] args) => new PostNotifierProgram().MainAsync().GetAwaiter().GetResult();
    }
}
